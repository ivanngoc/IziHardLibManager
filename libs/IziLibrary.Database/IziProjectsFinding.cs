using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;
using IziLibrary.Database;

namespace IziHardGames.Projects
{
    public class ItemsInFolder
    {
        public InfoIziProjectsMeta? meta = null;
        public InfoDll? infoDll = null;
        public InfoPackageJson? packageJson = null;
        public InfoAsmdef? infoAsmdef = null;
        public InfoUnityMeta? infoUnityMeta = null;
        public InfoCsproj? infoCsproj = null;
        public InfoSln? infoSln = null;
    }

    public static class IziProjectsFinding
    {
        public const string META_NAME = "iziprojects.json";
        public static IEnumerable<FileInfo> SearchAsmdefsNested(DirectoryInfo dir)
        {
            var asmdefs = dir.SelectAllFilesBeneath().Where(x => InfoAsmdef.IsValidExtension(x.Extension));
            return asmdefs;
        }
        public static async Task SearchOnly(DirectoryInfo dir, List<InfoBase> result, int depth, List<ItemsInFolder> itemsInFolders)
        {
            var dirs = dir.GetDirectories().Where(x => !x.IsJunction()).OrderBy(x => x.FullName);
            var files = dir.GetFiles();

            ItemsInFolder itemsInFolder = new ItemsInFolder();
            itemsInFolders.Add(itemsInFolder);

            foreach (var fileInfo in files)
            {
                InfoBase? infoBase = default;

                if (fileInfo.Name == META_NAME)
                {
                    infoBase = itemsInFolder.meta = new InfoIziProjectsMeta(fileInfo);
                }
                else if (fileInfo.Name == InfoDll.FILE_NAME)
                {
                    infoBase = itemsInFolder.infoDll = new InfoDll(fileInfo);
                }
                else if (fileInfo.Name.ToLowerInvariant() == InfoPackageJson.FILE_NAME.ToLowerInvariant())
                {
                    var temp = new InfoPackageJson(fileInfo);
                    if (await InfoPackageJson.IsFromUnityAsync(fileInfo))
                    {
                        infoBase = itemsInFolder.packageJson = temp;
                    }
                    else continue;
                }
                else
                {
                    switch (fileInfo.Extension)
                    {
                        case ".asmdef":
                            {
                                infoBase = itemsInFolder.infoAsmdef = new InfoAsmdef(fileInfo);
                                break;
                            }
                        case ".csproj":
                            {
                                infoBase = itemsInFolder.infoCsproj = new InfoCsproj(fileInfo);
                                break;
                            }
                        case ".sln":
                            {
                                infoBase = itemsInFolder.infoSln = new InfoSln(fileInfo);
                                break;
                            }
                        default: break;
                    }
                }
                if (infoBase == null || infoBase.IsSkipped) continue;
                result.Add(infoBase);
                Console.WriteLine($"Finded: {infoBase.GetType().Name}. {infoBase.FileInfo.FullName}");
                if (depth == 1)
                {
                    infoBase.IsRoot = true;
                }
            }
            foreach (var subdir in dirs)
            {
                await SearchOnly(subdir, result, depth + 1, itemsInFolders).ConfigureAwait(false);
            }
        }
        public static async Task<List<ItemsInFolder>> MainSearchAsync(DirectoryInfo dir, List<InfoBase> result, int depth, List<InfoRelation> infoRelations)
        {
            List<ItemsInFolder> pairs = new List<ItemsInFolder>();

            await SearchOnly(dir, result, depth, pairs).ConfigureAwait(false);

            foreach (var item in result)
            {
                await item.ExecuteAsync().ConfigureAwait(false);
            }

            foreach (var pair in pairs)
            {
                if (pair.infoAsmdef != null && pair.infoCsproj != null)
                {
                    var relation = new InfoRelation();
                    relation.guid = Guid.NewGuid();
                    relation.from = pair.infoAsmdef;
                    relation.to = pair.infoCsproj;
                    relation.flags |= ERelationsFlags.Correspond;
                    infoRelations.Add(relation);
                }

                if (pair.infoAsmdef != null && pair.infoUnityMeta != null)
                {
                    var relation = new InfoRelation();
                    relation.guid = Guid.NewGuid();
                    relation.from = pair.infoAsmdef;
                    relation.to = pair.infoUnityMeta;
                    relation.flags |= ERelationsFlags.Meta;
                    infoRelations.Add(relation);
                }
            }
            return pairs;
        }

        /// <summary>
        /// Считывает из файлов ссылки на зависимости от третьих сторон (DLL или пакеты юнити).
        /// Создает <see cref="InfoDependecy"/> этих зависимостей
        /// </summary>
        /// <param name="config"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task SearchForDependeciesForeignAsync(JsonObject config, List<InfoBase> result, List<InfoAsmdef> unityCache)
        {
            var myInfos = result.ToArray();
            var myAsmdefs = result.Select(x => x as InfoAsmdef).Where(x => x != null).ToArray();
            var myCsprojs = result.Select(x => x as InfoCsproj).Where(x => x != null);

            var dlls = SearchForDlls(config);
            var dllsDependeies = SearchForCsprojDlls(myCsprojs!);


            var finalDlls = JoinDlls(dlls, dllsDependeies);
            result.AddRange(finalDlls);


            var asmdefs = await SearchForForeignAsmdefsDependecies(config, myAsmdefs!, unityCache);
            result.AddRange(asmdefs);
        }

        /// <summary>
        /// Объединяет метаданные фактически существующих файлов с метаданными от непроверенных ссылок
        /// </summary>
        /// <param name="existedDependecies"></param>
        /// <param name="referencedDependecies"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<InfoDependecy> JoinDlls(InfoDependecy[] existedDependecies, List<InfoDependecy> referencedDependecies)
        {
            List<InfoDependecy> result = new List<InfoDependecy>();
            foreach (var refed in referencedDependecies)
            {
                foreach (var existed in existedDependecies)
                {
                    if (existed.SameSource(refed))
                    {
                        // на existed может ссылаться несколько refed. но запишется guid из последнего refed у которо найден guid
                        InfoDependecy joined = InfoDependecy.MergeMainWithSub(existed, refed);
                        if (!result.Contains(existed))
                        {
                            result.Add(joined);
                        }
                        goto NEXT;
                    }
                }
                throw new Exception($"No dll exist in root folder for ref: {refed.FileInfo!.FullName}");
                NEXT: continue;
            }
            return result;
        }

        public static async Task<List<InfoDependecy>> SearchForForeignAsmdefsDependecies(JsonObject config, IEnumerable<InfoAsmdef> myAsmdefs, List<InfoAsmdef> resultAsmdefs)
        {
            List<InfoDependecy> result = new List<InfoDependecy>();

            await CollectAsmdefsFromCache(config, resultAsmdefs);

            foreach (var asmdef in resultAsmdefs)
            {
                if (myAsmdefs.Any(x => x!.IsUse(asmdef)))
                {
                    InfoDependecy infoDependecy = new InfoDependecy(asmdef.FileInfo!);
                    infoDependecy.ModuleType = EModuleType.UnityAsmdefThirdParty;
                    infoDependecy.SetGuid(asmdef.Guid);
                    result.Add(infoDependecy);
                }
            }
            return result;
        }
        public static InfoDependecy[] SearchForDlls(JsonObject config)
        {
            List<InfoDependecy> result = new List<InfoDependecy>();
            var pathDlss = (string)config["dlls"]!;
            DirectoryInfo directoryInfoDlls = new DirectoryInfo(pathDlss);
            List<FileInfo> dllFiles = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(directoryInfoDlls, dllFiles, x => x.Extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase));

            foreach (var file in dllFiles)
            {
                InfoDependecy infoDependecy = new InfoDependecy(file);
                infoDependecy.SetGuid(Guid.NewGuid());
                infoDependecy.ModuleType = EModuleType.Dll;
                result.Add(infoDependecy);
            }
            return result.ToArray();
        }

        public static async Task CollectAsmdefsFromCache(JsonObject config, List<InfoAsmdef> unitCache)
        {
            var dir = (string)config["dir_unity_packages_cache"]!;
            DirectoryInfo root = new DirectoryInfo(dir);
            List<InfoBase> results = new List<InfoBase>();
            List<InfoRelation> relations = new List<InfoRelation>();
            await MainSearchAsync(root, results, 0, relations);

            foreach (var item in results)
            {
                if (item is InfoAsmdef asmdef)
                {
                    unitCache.Add(asmdef);
                }
            }
        }


        /// <summary>
        /// each <see cref="InfoCsproj"/> supose to be initilized by <see cref="InfoCsproj.ExecuteAsync"/> before that call
        /// </summary>
        /// <param name="csprojs"></param>
        /// <returns></returns>
        public static List<InfoDependecy> SearchForCsprojDlls(IEnumerable<InfoCsproj> csprojs)
        {
            List<InfoDependecy> result = new List<InfoDependecy>();
            foreach (var csproj in csprojs)
            {
                if (!csproj.IsExecuted) throw new InvalidOperationException($"You must call {nameof(InfoCsproj)}.{nameof(InfoCsproj.ExecuteAsync)}() before that method");

                foreach (var item in csproj.Items)
                {
                    if (item.refType == ERefType.NetSdkReference)
                    {
                        Console.WriteLine($"{nameof(ERefType.NetSdkReference)} founded in: {csproj.FileInfo!.FullName}");
                        var path = item.pathToItemAbsolute;
                        FileInfo fileInfo = new FileInfo(path);
                        InfoDependecy infoDependecy = new InfoDependecy(fileInfo);
                        infoDependecy.ModuleType = EModuleType.ForeignDependecy;
                        infoDependecy.SetGuid(item.guid);
                        result.Add(infoDependecy);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Ищет по ссылкам внутри соответствующих файлов дургие файлы на которые указывают ссылки. 
        /// По заранее сформированному списку соотносит считанные ссылки с элементами из списка.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static List<InfoRelation> SearchForDependeciesInnerAsync(List<InfoBase> items)
        {
#if DEBUG
            var dlls = items.Where(x => x is InfoDependecy).Select(x => x as InfoDependecy).ToArray();
#endif
            List<InfoRelation> result = new List<InfoRelation>();
            foreach (var item in items)
            {
                if (item is InfoCsproj csproj)
                {
                    csproj.FindConnections(result, x =>
                    {
                        if (x.isGuidFinded)
                        {
                            var byGuid = items.FirstOrDefault(y => y.IsGuidFounded && y.Guid == x.guid);
                            if (byGuid != null) return byGuid;
                        }

                        var byPath = items.FirstOrDefault(y => UtilityForPath.SamePath(y.FileInfo!, x.pathToItemAbsolute));
                        if (byPath != null)
                        {
                            return byPath;
                        }

                        FileInfo fileInfo = new FileInfo(x.pathToItemAbsolute);
                        var byFileName = items.FirstOrDefault(y => y.FileInfo!.Name.Trim() == fileInfo.Name.Trim());
                        if (byFileName != null)
                        {
                            csproj.FixReferenceByFilename(byFileName as InfoCsproj ?? throw new InvalidCastException(), x);
                            return byFileName;
                        }
                        Config.EnsureLoaded();
                        var jReplace = Config.JObj["replace_filenames"]!.AsArray().FirstOrDefault(x => (string)x["from"]! == fileInfo.Name);
                        if (jReplace != null)
                        {
                            var targetFileName = (string)jReplace["to"]!;
                            var replaced = items.FirstOrDefault(x => x.FileInfo!.Name == targetFileName);
                            if (replaced != null)
                            {
                                csproj.FixReferenceByFilename(replaced as InfoCsproj ?? throw new InvalidCastException(), x);
                                return replaced;
                            }
                        }

                        throw new System.NotImplementedException($"Project: {item.ToStringInfo()}{Environment.NewLine}No Guid Founded or Path confirmed for:{Environment.NewLine}{x.ToStringInfo()}");
                    });
                }
                else if (item is InfoAsmdef asmdef)
                {
                    asmdef.FindConnectionsOwn(result, (x) =>
                    {
                        return items.Where(x => x is InfoAsmdef).Select(x => x as InfoAsmdef).First(y => string.Compare(y.Guid, x, StringComparison.InvariantCultureIgnoreCase) == 0)!;
                    });
                }
                else if (item is InfoSln sln)
                {
                    sln.FindConnections(result, (arg) =>
                    {
                        return items.Where(y => y is InfoCsproj).Select(y => y as InfoCsproj).First(y =>
                        {
                            if (arg.isGuidFinded)
                            {
                                if (string.Equals(arg.guid, y!.Guid, StringComparison.InvariantCultureIgnoreCase)) return true;
                            }
                            if (UtilityForPath.SamePath(y!.FileInfo!, arg.pathToItemAbsolute))
                            {
                                return true;
                            }
                            return false;
                        })!;
                    });
                }
                else if (item is InfoPackageJson infoPackageJson)
                {
                    infoPackageJson.FindConnections(result, (name, version) =>
                    {
                        return items.Where(z => z is InfoPackageJson).Select(z => z as InfoPackageJson).First(z =>
                        {
                            if (z.GetPackageName() == name) return true;
                            return false;
                        })!;
                    });
                }
                else if (item is InfoUnityMeta meta)
                {
                    meta.PutRelation(result);
                }
                else if (item is InfoDependecy dependecy)
                {
                    // nothing 
                }
            }
            return result;
        }



        public static async ValueTask<List<InfoCsproj>> FindNestedCsprojAsync(DirectoryInfo root)
        {
            var list = new List<InfoCsproj>();
            await FindCsprojWithNestedOptionAsync(root, list);
            return list;
        }

        public static async Task FindDependeciesForCsproj(DirectoryInfo root, JsonObject config, List<InfoBase> infos, List<InfoAsmdef> unityPackagesCache)
        {
            foreach (var info in infos)
            {
                if (info is InfoCsproj csproj)
                {
                    if (csproj.TryGetAsmdef(out InfoAsmdef asmdef))
                    {
                        if (!asmdef!.IsNoUnityEngineRefs)
                        {
                            await csproj.EnsureRefToUnityCore(config);
                        }

                        var guids = asmdef.Refs;
                        List<InfoAsmdef> refs = new List<InfoAsmdef>();

                        foreach (var guid in guids)
                        {
                            var refAsmdef = infos.Select(x => x as InfoAsmdef).Where(x => x != null).First(x => x!.Guid == guid)!;
                            refs.Add(refAsmdef);
                        }

                        foreach (var refAsmdef in refs)
                        {
                            if (refAsmdef.infoProj == null) throw new NullReferenceException();
                            var refProj = refAsmdef.infoProj;
                            await csproj.EnsureRefToAsync(refAsmdef, refProj);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Associated Asmdef");
                    }
                }
            }
            throw new System.NotImplementedException();
        }

        public static async Task FindCsprojWithNestedOptionAsync(DirectoryInfo dir, List<InfoCsproj> result)
        {
            var projs = dir.GetFiles().Where(x => x.Extension == ".csproj");
            var dirs = dir.GetDirectories();


            var indexStart = result.Count;

            foreach (var file in projs)
            {
                var info = new InfoCsproj(file);
                info.IsNestingSearched = true;
                result.Add(info);
            }

            var indexBeginNested = result.Count;

            foreach (var subdir in dirs)
            {
                await FindCsprojWithNestedOptionAsync(subdir, result);
            }

            var indexEndNested = result.Count;

            for (int i = indexStart; i < indexBeginNested; i++)
            {
                for (int j = indexBeginNested; j < indexEndNested; j++)
                {
                    result[i].AddNested(result[j]);
                }
            }
        }

        public static CsprojFindResult FindAndUpdateCsproj(Guid guid, string pathAbs, string projectFileName, string projectName)
        {
            FileInfo fileInfo = new FileInfo(pathAbs);
            DirectoryInfo? directoryInfo = fileInfo.Directory!;
            CsprojFindResult result = new CsprojFindResult();
            using ModulesDbContext context = new ModulesDbContext();

            bool isActualGuid = default;
            bool isActualPathAbs = default;
            bool isActualProjectFileName = default;
            bool isActualProjectName = default;

            if (directoryInfo != null && directoryInfo.Exists)
            {
                result.isExistsAny = true;
            }

            IziModelCsproj? csproj = default;

            if (context.TryFindByGuid(guid, out csproj))
            {
                result.isExistsInDataBase = true;
            }
            else if (context.TryFindByAbsPath(pathAbs, out csproj))
            {
                throw new System.NotImplementedException();
            }
            else if (context.TryFindByProjectFileName(projectFileName, out csproj))
            {
                throw new System.NotImplementedException();
            }
            else if (context.TryFindByProjectName(projectName, out csproj))
            {
                throw new System.NotImplementedException();
            }
            if (csproj != null)
            {
                result.actualGuid = csproj.Module!.Guid;
            }
            else
            {
                throw new System.NotImplementedException();
            }
            return result;
        }

        public static async Task<List<InfoBase>> FindAllFromConfig()
        {
            await Config.LoadAsync();
            var result = new List<InfoBase>();
            var pairs = new List<ItemsInFolder>();

            foreach (var item in Config.SearchDirs)
            {
                var dir = new DirectoryInfo(item);
                if (dir.Exists)
                {
                    await SearchOnly(dir, result, 0, pairs);
                }
            }
            return result;
        }

        public static FileInfo? FindMainPackageJson(DirectoryInfo directory)
        {
            var packJson = directory.GetFiles().FirstOrDefault(x => InfoPackageJson.IsValid(x));
            if (packJson != null)
            {
                return FindMainPackageJson(packJson);
            }
            else
            {
                DirectoryInfo? dir = directory.Parent;
                while (dir != null)
                {
                    var file = dir.GetFiles().FirstOrDefault(x => InfoPackageJson.IsValid(x));
                    if (file != null) return FindMainPackageJson(file);
                    dir = dir.Parent;
                }
            }
            return null;
        }
        public static FileInfo FindMainPackageJson(FileInfo packJson)
        {
            FileInfo root = packJson;
            DirectoryInfo? dir = packJson.Directory!.Parent;

            while (dir != null)
            {
                var file = dir.GetFiles().FirstOrDefault(x => InfoPackageJson.IsValid(x));
                if (file != null) root = file;
                dir = dir.Parent;
            }
            return root;
        }

        public struct CsprojFindResult
        {
            public bool isExistsAny;
            public bool isExistsInDataBase;
            public string actualPathAbs;
            public string actualProjectFileName;
            public string actualProjectName;
            public Guid actualGuid;
        }

        public static async ValueTask<InfoIziProjectsMeta> CreateDefaultFileAsync(DirectoryInfo directory)
        {
            Guid guid = System.Guid.NewGuid();
            var infos = new List<InfoBase>();
            FileInfo fileInfo = new FileInfo(Path.Combine(directory.FullName, META_NAME));
            InfoIziProjectsMeta meta = new InfoIziProjectsMeta(fileInfo);
            meta.SetGuidGenerated(guid);

            var pairs = new List<ItemsInFolder>();

            await SearchOnly(directory, infos, 256, pairs).ConfigureAwait(false);

            foreach (var item in infos)
            {
                if (item is InfoCsproj infoCsproj)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(infoCsproj);
                }
                else if (item is InfoAsmdef infoAsmdef)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(infoAsmdef);
                }
                else if (item is InfoPackageJson packageJson)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(packageJson);
                }
            }
            await File.WriteAllTextAsync(fileInfo.FullName, meta.ToString()).ConfigureAwait(false);
            return meta;
        }
    }
}