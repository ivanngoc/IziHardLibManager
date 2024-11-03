using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.Sln;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziEnsureSlnPack
    {
        public static async Task FormPackSlnDefault()
        {
            await FormPackSln(new FileInfo(@"C:\.izhg-lib\packs.sln"), new DirectoryInfo("C:\\.izhg-lib")).ConfigureAwait(false);
        }
        public static async Task FormPackSln(FileInfo target, DirectoryInfo scandDir)
        {
            if (target.Exists)
            {
                UtilityForFileInfo.Backup(target);
            }
            var csprojs = scandDir.SelectAllFilesBeneath().Where(x => InfoCsproj.IsValidExtension(x));
        }

        public static Task EnsureDependecies()
        {
            return EnsureDependecies(new FileInfo(@"C:\.izhg-lib\packs.sln"));
        }
        public static async Task EnsureDependecies(FileInfo packsln)
        {
            if (packsln.Exists)
            {
                using ModulesDbContextV1 context = new ModulesDbContextV1();

                InfoSln infoSln = new InfoSln(packsln);
                await infoSln.ExecuteAsync().ConfigureAwait(false);
                UtilityForFileInfo.Backup(packsln);

                foreach (var item in infoSln.Items)
                {
                    if (item.refType == ERefType.SlnCsproj)
                    {
                        FileInfo fiCsproj = new FileInfo(item.pathToItemAbsolute);
                        if (fiCsproj.Exists)
                        {
                            InfoCsproj csproj = new InfoCsproj(fiCsproj);
                            await csproj.ExecuteAsync().ConfigureAwait(false);
                            var ifAsmdef = fiCsproj.Directory!.GetFiles().FirstOrDefault(x => OldInfoAsmdef.IsValidExtension(x));
                            if (ifAsmdef != null && ifAsmdef.Exists)
                            {
                                OldInfoAsmdef asmdef = new OldInfoAsmdef(ifAsmdef);
                                await asmdef.ExecuteAsync().ConfigureAwait(false);

                                foreach (var refGuid in asmdef.Refs)
                                {
                                    Guid guid = Guid.Parse(refGuid);
                                    var modelDepAsmdef = context.UnityAsmdefs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == guid);
                                    if (modelDepAsmdef != null)
                                    {
                                        var fiAsmdefDep = new FileInfo(modelDepAsmdef.PathFull);
                                        var fiCsprojDep = fiAsmdefDep.Directory!.GetFiles().FirstOrDefault(x => InfoCsproj.IsValidExtension(x));
                                        if (fiCsprojDep != null)
                                        {
                                            Console.WriteLine($"{typeof(IziEnsureSlnPack).Name}: Ensure csproj: {fiCsprojDep.FullName}");
                                            await SlnMappedFile.EnsureCsprojReferencedAsync(packsln, fiCsprojDep).ConfigureAwait(false);
                                        }
                                    }
                                    else
                                    {
                                        throw new NullReferenceException($"Can't Find Asmdef Model in database with guid:{refGuid}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new FileNotFoundException(fiCsproj.FullName);
                        }
                    }
                }
            }
            else
            {
                throw new FileNotFoundException(packsln.FullName);
            }
        }
    }
}