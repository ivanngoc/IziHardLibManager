using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase.Models;
using IziProjectsManager.DataBase;
using Microsoft.EntityFrameworkCore;
using static IziHardGames.Projects.DataBase.ConstantsForIziProjects;

namespace IziHardGames.Projects.DataBase
{
    public class ModulesDbContext : DbContext, IDataBaseAdapter
    {
        public DbSet<IziModelCsproj> Csprojs { get; set; }
        public DbSet<IziModelModule> Modules { get; set; }
        public DbSet<IziModelUnityAsmdef> UnityAsmdefs { get; set; }
        public DbSet<IziModelSln> Slns { get; set; }
        public DbSet<IziModelUnityPackageJson> UnityPackageJsons { get; set; }
        public DbSet<IziModelUnityMeta> UnityMetas { get; set; }
        public DbSet<IziModelDependecy> Dependecies { get; set; }
        public DbSet<IziModelRelation> Relations { get; set; }
        public DbSet<IziModelInfoDll> InfosForDlls { get; set; }
        public DbSet<IziModelDll> Dlls { get; set; }
        public DbSet<IziModelMeta> IziMetas { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        public static async Task InitDllsAsync(List<InfoBase> infoBases)
        {
            using ModulesDbContext context = new ModulesDbContext();
            foreach (var item in infoBases)
            {
                if (item is InfoDll infoDll)
                {
                    foreach (var dll in infoDll.Dlls)
                    {
                        await context.AddAsync(dll).ConfigureAwait(false);
                    }
                }
            }
        }
        public static async Task ResolveDependecies()
        {
            using ModulesDbContext context = new ModulesDbContext();
            await ResolveDependecies(context).ConfigureAwait(false);
        }
        public static async Task ResolveDependecies(ModulesDbContext context)
        {
            //foreach (var relation in relations)
            //{
            //    await context.AddAsync(relation);
            //}

            //await context.SaveChangesAsync().ConfigureAwait(false);

            //if (false)
            //{
            //    var relations2 = IziProjectsFinding.SearchForDependeciesInnerAsync(list);

            //    foreach (var item in relations2)
            //    {
            //        await context.AddAsync(item).ConfigureAwait(false);
            //    }
            //    await context.SaveChangesAsync().ConfigureAwait(false);
            //    await context.CorrespondingAllAsync().ConfigureAwait(false);
            //}
        }
        public static async Task InitAsync()
        {
            Console.WriteLine($"Begin Init database");
            using ModulesDbContext context = new ModulesDbContext();
            await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
            await context.Database.EnsureCreatedAsync().ConfigureAwait(false);

            await Config.LoadAsync(Config.GetConfigPath()).ConfigureAwait(false);

            var infos = new List<InfoBase>();
            var relations = new List<InfoRelation>();
            var itemsPerFolder = new List<ItemsInFolder>();

            foreach (var dir in Config.SearchDirs)
            {
                var dirInfo = new DirectoryInfo(dir);

                if (dirInfo.Exists)
                {
                    await IziProjectsFinding.SearchOnly(dirInfo, infos, 256, itemsPerFolder).ConfigureAwait(false);
                }
            }
            int counter = 0;
            foreach (var info in infos)
            {
                Console.WriteLine($"InitDatabase. Item:{counter}");
                await info.ExecuteAsync().ConfigureAwait(false);
                counter++;
            }
            Console.WriteLine("InitDb. Complete search");
            IziProjectsValidations.EnsureGuid(infos);

            if (false)
            {
                List<InfoAsmdef> unityCache = new List<InfoAsmdef>();
                await IziProjectsFinding.SearchForDependeciesForeignAsync(Config.JObj!, infos, unityCache).ConfigureAwait(false);

                foreach (var item in unityCache)
                {
                    item.IsThirdParty = true;
                }
                infos.AddRange(unityCache);
            }

            foreach (var item in infos)
            {
                if (item is InfoCsproj csproj)
                {
                    await context.AddAsync(csproj).ConfigureAwait(false);
                }
                else if (item is InfoAsmdef asmdef)
                {
                    await context.AddAsync(asmdef).ConfigureAwait(false);
                }
                else if (item is InfoSln sln)
                {
                    await context.AddAsync(sln).ConfigureAwait(false);
                }
                else if (item is InfoPackageJson infoPackageJson)
                {
                    await context.AddAsync(infoPackageJson).ConfigureAwait(false);
                }
                else if (item is InfoUnityMeta meta)
                {
                    await context.AddAsync(meta).ConfigureAwait(false);
                }
                else if (item is InfoDependecy dependecy)
                {
                    await context.AddAsync(dependecy).ConfigureAwait(false);
                }
                else if (item is InfoDll infoDll)
                {
                    await context.AddAsync(infoDll).ConfigureAwait(false);
                }
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
            Console.WriteLine($"Init Database Completed");
        }

        public async Task CorrespondingAllAsync()
        {
            await CorrespondingAsmdefToCsprojAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Ищет соответствия между файлами
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public async Task CorrespondingAsmdefToCsprojAsync()
        {
            var asmdefs = UnityAsmdefs.Include(x => x.Module).ToArray();

            foreach (var asmdef in asmdefs)
            {
                FileInfo fileInfo = new FileInfo(asmdef.PathFull);
                var queue = Csprojs.Include(x => x.Module).ToArray();
                var sameFolder = queue.Where(x => UtilityForPath.IsSameDir(new FileInfo(x.PathFull).Directory!, fileInfo.Directory!)).FirstOrDefault();
                if (sameFolder != null)
                {
                    var connection = Relations.FirstOrDefault(x => x.From == asmdef.Module && x.To == sameFolder.Module);
                    if (connection == null)
                    {
                        IziModelRelation iziConnection = new IziModelRelation()
                        {
                            From = asmdef.Module,
                            To = sameFolder.Module,
                            RelationFlags = (long)ERelationsFlags.Correspond,
                        };
                        Relations.Add(iziConnection);
                    }
                    else
                    {
                        connection.RelationFlags = connection.RelationFlags | (long)ERelationsFlags.Correspond;
                    }
                }
            }
            await SaveChangesAsync();
        }

        public async Task AddAsync(InfoCsproj info)
        {
            var model = new IziModelCsproj(info);
            await Csprojs.AddAsync(model);
        }
        public async Task AddAsync(InfoAsmdef info)
        {
            var model = new IziModelUnityAsmdef(info);
            await UnityAsmdefs.AddAsync(model);
        }
        public async Task AddAsync(InfoSln info)
        {
            var model = new IziModelSln(info);
            await Slns.AddAsync(model);
        }
        public async Task AddAsync(InfoPackageJson info)
        {
            var model = new IziModelUnityPackageJson(info);
            await UnityPackageJsons.AddAsync(model);
        }
        public async Task AddAsync(InfoUnityMeta meta)
        {
            var model = new IziModelUnityMeta(meta);
            await UnityMetas.AddAsync(model);
        }
        public async Task AddAsync(InfoDll item)
        {
            var model = new IziModelInfoDll(item);
            await InfosForDlls.AddAsync(model).ConfigureAwait(false);
            await AddOrUpdateDllsFromInfo(item).ConfigureAwait(false);
        }
        public async Task AddAsync(InfoRelation connection)
        {
            var from = Modules.First(x => x.Guid == connection.from!.GuidStruct);
            var to = Modules.First(x => x.Guid == connection.to!.GuidStruct);

            var model = new IziModelRelation();
            model.From = from;
            model.To = to;
            model.RelationFlags = (long)connection.flags;

            await Relations.AddAsync(model);
        }
        public async Task AddAsync(InfoDependecy dependecy)
        {
            var model = new IziModelDependecy(dependecy);
            await Dependecies.AddAsync(model);
        }

        public IziModelModule GetModuleByGuid(Guid guid)
        {
            return Modules.First(x => x.Guid == guid);
        }

        public IziModelCsproj[] GetDependeciesCsproj(IziModelCsproj csproj)
        {
            throw new System.NotImplementedException();
        }
        public IziModelUnityAsmdef[] GetDependecies(IziModelUnityAsmdef asmdef)
        {
            throw new NotImplementedException();
        }

        public IziModelCsproj GetCorrespondCsproj(IziModelUnityAsmdef item)
        {
            IziModelModule to = Relations.Include(x => x.From).Include(x => x.To).Where(x => x.From!.Guid == item.Module!.Guid && ((ERelationsFlags)x.RelationFlags).HasFlag(ERelationsFlags.Correspond)).First().To!;
            return Csprojs.Include(x => x.Module).First(x => x.Module!.Guid == to.Guid);
        }

        public async Task AddNotIdentifiedGuid(Guid refGuidStruct, string target)
        {
            IziModelModule model = new IziModelModule();
            model.Guid = refGuidStruct;
            model.Flags = (long)EModuleFlags.Error;
            model.Type = (uint)EModuleType.Unknown;
            model.Description = $"Guid не распознан. Источник в: {target}";
            await Modules.AddAsync(model).ConfigureAwait(false);
        }

        public async Task AddOrUpdateDllsFromInfo(InfoDll infoDll)
        {
            var infos = await Dlls.Include(x => x.Module).ToArrayAsync().ConfigureAwait(false);
            foreach (var item in infoDll.Dlls)
            {
                if (infos.TryFindFirst(x => x.Module!.Guid == item.guid, out IziModelDll dll))
                {
                    dll.FileName = item.filename;
                    dll.PathFull = item.pathAbsolute;
                    dll.PathRelative = item.pathRelative;
                }
                else
                {
                    IziModelDll iziDll = new IziModelDll(item);
                    await Dlls.AddAsync(iziDll).ConfigureAwait(false);
                }
            }
        }
        public async Task AddOrUpdateAsync(InfoDll infoDll)
        {
            if (!InfosForDlls.Include(x => x.Module).Any(x => x.Module!.Guid == infoDll.GuidStruct))
            {
                await this.AddAsync(infoDll).ConfigureAwait(false);
            }
            else
            {
                await this.AddOrUpdateDllsFromInfo(infoDll).ConfigureAwait(false);
            }
        }

        public bool TryFindByGuid(Guid guid, out IziModelCsproj? csproj)
        {
            csproj = Csprojs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == guid);
            return csproj != null;
        }

        public bool TryFindByAbsPath(string pathAbs, out IziModelCsproj? csproj)
        {
            csproj = Csprojs.Include(x => x.Module).First(x => string.Equals(x.PathFull, pathAbs, StringComparison.InvariantCultureIgnoreCase));
            return csproj != null;
        }

        public bool TryFindByProjectFileName(string projectFileName, out IziModelCsproj? csproj)
        {
            throw new NotImplementedException();
        }

        public bool TryFindByProjectName(string projectName, out IziModelCsproj? csproj)
        {
            throw new NotImplementedException();
        }

        public bool IsThirdPartyAsmdef(Guid guid)
        {
            return UnityAsmdefs.Include(x => x.Module).Any(x => x.IsThirdParty && x.Module!.Guid == guid);
        }
    }
}
