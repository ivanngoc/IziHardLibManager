using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetCore;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziEnsureAsmdef
    {
        public static async Task EnsureReferencesToUnityDll()
        {
            Console.WriteLine($"Begin {nameof(EnsureReferencesToUnityDll)}");
            using ModulesDbContext context = new ModulesDbContext();
            var asmdefs = await context.UnityAsmdefs.Include(x => x.Module).ToArrayAsync().ConfigureAwait(false);
            await Config.EnsureLoadedAsync().ConfigureAwait(false);
            var pathUnityCore = (string)(Config.JObj["unity_lib"]!["UnityEngine.CoreModule"]!);

            foreach (var item in asmdefs)
            {
                FileInfo fileInfo = new FileInfo(item.PathFull);
                InfoAsmdef infoAsmdef = new InfoAsmdef(fileInfo);
                await infoAsmdef.ExecuteAsync().ConfigureAwait(false);
                if (!infoAsmdef.IsNoUnityEngineRefs)
                {
                    var csproj = context.GetCorrespondCsproj(item);
                    FileInfo csprojFileInfo = new FileInfo(csproj.PathFull);
                    if (!csprojFileInfo.Exists) throw new FileNotFoundException(csproj.PathFull);
                    EnsureUnityDll(csprojFileInfo, pathUnityCore);
                    Console.WriteLine($"Did for {csprojFileInfo.FullName}");
                }
            }
            Console.WriteLine($"End {nameof(EnsureReferencesToUnityDll)}");
        }
        public static void EnsureUnityDll(FileInfo csprojFileInfo, string pathUnityCore)
        {
            ProjectRootElement element = ProjectRootElement.Open(csprojFileInfo.FullName);
            FileInfo dllUnityEngineCoreModule = new FileInfo(pathUnityCore);
            string pathRelative = UtilityForPath.AbsToRelative(csprojFileInfo.Directory!, pathUnityCore);

            if (!dllUnityEngineCoreModule.Exists) throw new FileNotFoundException(pathUnityCore);

            var elRef = element.Items.FirstOrDefault(x => x.ElementName == "Reference" && x.Include == "UnityEngine.CoreModule");
            if (elRef != null)
            {
                var elPath = elRef.Metadata.FirstOrDefault(x => x.ElementName == "HintPath");
                if (elPath != null)
                {
                    if (elPath.Value == pathRelative) return;
                    elPath.Value = pathRelative;
                }
                else
                {
                    elRef.AddMetadata("HintPath", pathRelative);
                }
            }
            else
            {
                Dictionary<string, string> meta = new Dictionary<string, string>()
                {
                    ["HintPath"] = pathRelative,
                };
                elRef = element.AddItem("Reference", pathUnityCore, meta);
                elRef.Include = "UnityEngine.CoreModule";
            }
            element.Save();
        }

        public static async Task EnsureAsmdefDependeciesInCsproj()
        {
            Console.WriteLine($"Begin {nameof(EnsureAsmdefDependeciesInCsproj)}");

            using ModulesDbContext context = new ModulesDbContext();
            var asmdefs = await context.UnityAsmdefs.Include(x => x.Module).Where(x => !x.IsThirdParty).ToArrayAsync().ConfigureAwait(false);
            var thirdParty = await context.UnityAsmdefs.Include(x => x.Module).Where(x => x.IsThirdParty).ToArrayAsync().ConfigureAwait(false);

            foreach (var item in asmdefs)
            {
                FileInfo fileInfo = new FileInfo(item.PathFull);
                InfoAsmdef infoAsmdef = new InfoAsmdef(fileInfo);
                await infoAsmdef.ExecuteAsync().ConfigureAwait(false);
                var csproj = context.GetCorrespondCsproj(item);

                Console.WriteLine($"Begin");
                Console.WriteLine($"{fileInfo.FullName}");
                if (infoAsmdef.Refs.Length > 0)
                {
                    Console.WriteLine(infoAsmdef.Refs.Aggregate((x, y) => x + Environment.NewLine + y));
                }
                else
                {
                    Console.WriteLine($"no Refs");
                }
                Console.WriteLine($"End");

                foreach (var refGuid in infoAsmdef.Refs)
                {
                    var refAsmdef = asmdefs.FirstOrDefault(x => x.Module!.Guid == Guid.Parse(refGuid));
                    if (refAsmdef != null)
                    {
                        var refCsproj = context.GetCorrespondCsproj(refAsmdef);
                        InfoCsproj from = new InfoCsproj(new FileInfo(csproj.PathFull));
                        InfoCsproj to = new InfoCsproj(new FileInfo(refCsproj.PathFull));

                        var t1 = from.ExecuteAsync();
                        var t2 = to.ExecuteAsync();
                        await Task.WhenAll(t1, t2).ConfigureAwait(false);
                        bool isChanges = from.EnsureRefToCsproj(to);

                        ProjectRootElement projectRootElement = ProjectRootElement.Open(csproj.PathFull);
                        var elRef = projectRootElement.Items.First(x => x.ElementName == "ProjectReference");
                        var elMeta = elRef.Metadata.FirstOrDefault(x => x.ElementName == "UnityAsmdefGuid");
                        if (elMeta != null)
                        {
                            if (elMeta.Value != refGuid)
                            {
                                elMeta.Value = refGuid;
                                isChanges = true;
                            }
                        }
                        else
                        {
                            elRef.AddMetadata("UnityAsmdefGuid", refGuid);
                            isChanges = true;
                        }

                        if (isChanges)
                        {
                            from.Proj.Save();
                        }
                        Console.WriteLine($"Linked{Environment.NewLine}{from.FileInfo!.FullName}{Environment.NewLine}{to.FileInfo!.FullName}");
                    }
                    else
                    {
                        var refGuidStruct = Guid.Parse(refGuid);
                        if (context.Modules.Any(x => x.Guid == refGuidStruct)) continue;
                        var foundedDep = thirdParty.FirstOrDefault(x => x.Module!.Guid == refGuidStruct);
                        if (foundedDep == null)
                        {
                            await context.AddNotIdentifiedGuid(refGuidStruct, item.PathFull);
                        }
                    }
                }
            }
            Console.WriteLine($"End {nameof(EnsureAsmdefDependeciesInCsproj)}");
        }

        public static async Task EnsureAsmdefFormatSingle(FileInfo file)
        {
            InfoAsmdef asmdef = new InfoAsmdef(file);
            await asmdef.ExecuteAsync().ConfigureAwait(false);

            if (asmdef.isIzhgGuidPresented)
            {
                if (asmdef.GuidStruct != asmdef.infoMeta!.GuidStruct)
                {
                    await asmdef.infoMeta.OverrideGuidInFileAsync(asmdef.GuidStruct).ConfigureAwait(false);
                }
            }
            else
            {
                await asmdef.WriteGuidToFileAsync(asmdef.infoMeta!.GuidStruct).ConfigureAwait(false);
            }
        }


        public static async Task EnsureFormatHrf(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                bool isNeedToOverrideFile = false;
                string text = await File.ReadAllTextAsync(fileInfo.FullName);
                var jObj = JsonNode.Parse(text)!.AsObject();
                //var meta = jObj[InfoAsmdef.PROP_REF_HRF]!.AsArray();
                var refs = jObj[InfoAsmdef.PROP_REFS]!.AsArray();

                //isNeedToOverrideFile = meta.Count != refs.Count;
                using ModulesDbContext context = new ModulesDbContext();

                JsonArray jsonArray = new JsonArray();
                for (int i = 0; i < refs.Count; i++)
                {
                    var guid = InfoAsmdef.FindGuid(refs[i]!);
                    var dep = context.UnityAsmdefs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == guid);
                    string hrf = "NOT_FOUNDED_IN_DATA_BASE";
                    if (dep != null)
                    {
                        hrf = dep.Module!.Name;
                    }
                    jsonArray.Add(new JsonObject() { [guid.ToString("N")] = hrf });
                }
                jObj[InfoAsmdef.PROP_REF_HRF] = jsonArray;
                await File.WriteAllTextAsync(fileInfo.FullName, jObj.ToJsonString(Shared.jOptions));
            }
            else throw new FileNotFoundException(fileInfo.FullName);
        }

        public static async Task EnsureDependeciesJunctionsAsync(string asmdef)
        {
            FileInfo fileInfo = new FileInfo(asmdef);
            var unityProjectRoot = fileInfo.Directory;

            while (unityProjectRoot != null)
            {
                var dirs = unityProjectRoot.GetDirectories();
                if (dirs.Any(x => string.Equals(x.Name, "Assets", StringComparison.InvariantCultureIgnoreCase)) && dirs.Any(x => string.Equals(x.Name, "ProjectSettings", StringComparison.InvariantCultureIgnoreCase)))
                {
                    break;
                }
                unityProjectRoot = unityProjectRoot.Parent;
            }

            if (unityProjectRoot != null)
            {
                var pacakgesDir = new DirectoryInfo(Path.Combine(unityProjectRoot.FullName, "Packages"));
                if (pacakgesDir.Exists)
                {
                    DirectoryInfo dirCustomPackages = new DirectoryInfo(Path.Combine(unityProjectRoot.FullName, $"{Path.DirectorySeparatorChar}Packages{Path.DirectorySeparatorChar}"));
                    await EnsureDependeciesJunctionsAsync(unityProjectRoot, pacakgesDir, fileInfo).ConfigureAwait(false);
                    await EnsureDependeciesJunctionsAsync(pacakgesDir, pacakgesDir).ConfigureAwait(false);
                }
                else
                {
                    throw new DirectoryNotFoundException(pacakgesDir.FullName);
                }

            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
        public static async Task EnsureDependeciesJunctionsAsync(DirectoryInfo directory, DirectoryInfo targetDir, FileInfo rootAsmdef)
        {
            InfoAsmdef infoAsmdef = new InfoAsmdef(rootAsmdef);
            await infoAsmdef.ExecuteAsync().ConfigureAwait(false);
            using ModulesDbContext context = new ModulesDbContext();

            foreach (var item in infoAsmdef.Refs)
            {
                var guid = Guid.Parse(item);
                var targetAsmdef = context.UnityAsmdefs.Include(x => x.Module).FirstOrDefault(x => !x.IsThirdParty && x.Module!.Guid == guid);
                if (targetAsmdef != null)
                {
                    if (targetAsmdef.IsThirdParty) continue;

                    var targetAsmdefFi = new FileInfo(targetAsmdef.PathFull);
                    if (targetAsmdefFi.Exists)
                    {
                        var packJson = await EnsurePackageSymbolicLink(targetDir, targetAsmdefFi).ConfigureAwait(false);
                        await EnsureDependeciesJunctionsAsync(directory, targetDir, targetAsmdefFi);
                    }
                    else
                    {
                        Console.WriteLine($"Can't Find asmdef file {targetAsmdefFi.FullName}    {typeof(IziEnsureAsmdef).FullName}");
                    }
                }
                else
                {
                    Console.WriteLine($"Can't Find asmdef:{guid} in Database.   {typeof(IziEnsureAsmdef).FullName}");
                }
            }
        }

        private static async ValueTask<FileInfo> EnsurePackageSymbolicLink(DirectoryInfo targetDir, FileInfo targetAsmdefFi)
        {
            Console.WriteLine($"{nameof(EnsurePackageSymbolicLink)}()   {typeof(IziEnsureAsmdef).FullName}");
            var fiPackJson = await InfoPackageJson.FindMineAsync(targetAsmdefFi.Directory!);

            if (fiPackJson != null)
            {
                fiPackJson = IziProjectsFinding.FindMainPackageJson(fiPackJson);

                if (!targetDir.GetDirectories().Any(x => string.Equals(x.Name, fiPackJson.Directory!.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Console.WriteLine($"Create Symbolic Links: dirSource:{fiPackJson.Directory!.FullName}; target:{targetDir.FullName}");
                    UtilityForMklink.JunctionTargetDirToDir(fiPackJson.Directory!, targetDir);
                }
                else
                {
                    Console.WriteLine($"Directory with name:{fiPackJson.Directory!.Name} is Exist in dir:{targetDir.FullName}. No Need to create symbolic link");
                }
            }
            else
            {
                throw new NotImplementedException($"For Asmdef: {targetAsmdefFi.FullName} package.json is not founded");
            }
            return fiPackJson;
        }

        /// <summary>
        /// Для все файлов .asmdef в директории и вложенных директориях. Junctions создаются в targetDir
        /// </summary>
        /// <param name="packagesDir"></param>
        /// <param name="targetDir"></param>
        /// <returns></returns>
        public static async Task EnsureDependeciesJunctionsAsync(DirectoryInfo packagesDir, DirectoryInfo targetDir)
        {
            REPEAT:
            Console.WriteLine($"{nameof(EnsureDependeciesJunctionsAsync)}. directory:{packagesDir.FullName}. TargetDir:{targetDir.FullName}");
            var dirs = new List<DirectoryInfo>();
            var asmdefs = packagesDir.SelectAllFilesBeneath().Where(x => InfoAsmdef.IsValidExtension(x.Extension));
            Console.WriteLine($"Founded asmdefs:{Environment.NewLine}{asmdefs.Select(x => x.FullName).Aggregate((x, y) => x + Environment.NewLine + y)}");
            bool repeat = false;

            foreach (var item in asmdefs)
            {
                repeat |= await EnsureDependeciesJunctionsAsync(item, targetDir).ConfigureAwait(false);
            }
            if (repeat) goto REPEAT;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asmdefSource"></param>
        /// <returns></returns>
        public static async Task<bool> EnsureDependeciesJunctionsAsync(FileInfo asmdefSource, DirectoryInfo target)
        {
            InfoAsmdef infoAsmdef = new InfoAsmdef(asmdefSource);
            await infoAsmdef.ExecuteAsync().ConfigureAwait(false);
            using ModulesDbContext context = new ModulesDbContext();
            bool isChanges = false;

            foreach (var guidAsString in infoAsmdef.Refs)
            {
                Guid guid = Guid.Parse(guidAsString);
                var asmdefTargetModel = context.UnityAsmdefs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == guid);

                if (asmdefTargetModel == null)
                {
                    Console.WriteLine($"Can't Find asmdef with guid:{guid} at database. Is Third Party?:{context.IsThirdPartyAsmdef(guid)}");
                    continue;
                }

                if (asmdefTargetModel.IsThirdParty) continue;

                FileInfo asmdefTargetFi = new FileInfo(asmdefTargetModel.PathFull);

                if (asmdefTargetFi.Exists)
                {
                    Console.WriteLine($"dep exists. {asmdefTargetModel.PathFull}. guid:{asmdefTargetModel.Module!.Guid.ToString("D")} .{typeof(IziEnsureAsmdef).FullName}");
                    var dirAsmdefTarget = asmdefTargetFi.Directory!;

                    var fiPackJson = await InfoPackageJson.FindMineAsync(dirAsmdefTarget);

                    if (fiPackJson != null)
                    {
                        fiPackJson = IziProjectsFinding.FindMainPackageJson(fiPackJson);

                        Console.WriteLine($"Founded package.json:{fiPackJson.FullName} .{typeof(IziEnsureAsmdef).FullName}");

                        var dirPackJson = fiPackJson.Directory!;
                        var dirs = target.GetDirectories();
                        if (!dirs.Any(x => x.Name == dirPackJson.Name))
                        {
                            Console.WriteLine($"Create Symbolic Links: dirSource:{dirPackJson.FullName}; target:{target.FullName}");
                            UtilityForMklink.JunctionTargetDirToDir(dirPackJson, target);
                            isChanges = true;
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException($"For asmdef:[{asmdefTargetFi.FullName}] no package.json above");
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            return isChanges;
        }

    }
}