﻿using System;
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
    public static class IziProjectsOperations
    {
        public static async Task OrginizeForNowAsync(DirectoryInfo rootDir, List<ProjectInfo> binds, List<InfoBase> infos)
        {
            foreach (var info in infos)
            {
                if (info is OldInfoAsmdef asmdef)
                {
                    var currentDir = info.FileInfo!.Directory!;
                    if (!info.IsPaired)
                    {
                        CreateCsproj(rootDir, asmdef);
                    }

                    var files = currentDir.GetFiles();
                    var packJson = files.FirstOrDefault(x => x.Name.ToLowerInvariant() == InfoPackageJson.FILE_NAME);
                    if (packJson == null)
                    {
                        await CreatePackJsonAsync(rootDir, asmdef);
                    }

                    if (currentDir.FullName.ToLowerInvariant().StartsWith(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\_Engine".ToLowerInvariant()) ||
                       currentDir.FullName.ToLowerInvariant().StartsWith(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\_NonEngine".ToLowerInvariant()))
                    {
                        int level = DirDifferenceLevel(rootDir, info.FileInfo!.Directory!);
                        if (level > 0)
                        {
                            if (level > 1)
                            {
                                int countMoveBacks = level - 1;
                                DirectoryInfo target = info.FileInfo!.Directory!;
                                for (int i = 0; i < countMoveBacks; i++)
                                {
                                    target = target.Parent!;
                                }
                                if (target!.Parent!.FullName != rootDir.FullName) throw new InvalidOperationException();
                                info.SetMoveDirection(rootDir);
                            }
                        }
                        else
                        {
                            throw new FormatException($"Project: {info.FileInfo!.FullName!} is at root direction or outside pack folder. This is not allowed");
                        }
                        if (asmdef.TryMove())
                        {

                        }
                    }
                }
            }
        }
        public static void CreateCsproj(DirectoryInfo rootDir, OldInfoAsmdef asmdef)
        {
            var projName = asmdef.FileInfo!.Name;
            var template = rootDir.GetFiles().First(x => x.Name == "izhg.csproj.template");
            var newCsproj = template.CopyTo(Path.Combine(asmdef.FileInfo!.Directory!.FullName, $"{projName}.csproj"));
            Guid guid = Guid.NewGuid();
            ProjectRootElement proj = global::Microsoft.Build.Construction.ProjectRootElement.Open(newCsproj.FullName);
            proj.AddProperty("ProjectName", projName);
            proj.AddProperty("ProjectGuid", guid.ToString());
            proj.AddProperty("Authors", "Tran Ngoc Anh");
            proj.AddProperty("Company", "IziHardGames");
            proj.AddProperty("AsmdefGuid", asmdef.Guid);
            proj.AddProperty("AsmdefName", asmdef.FileInfo.Name);
            proj.Save();
            Console.WriteLine($"Created csporj:{newCsproj.FullName}");
        }
        public static async Task CreatePackJsonAsync(DirectoryInfo dir)
        {
            await IziProjectsOperations.CreateNewAsync(dir).ConfigureAwait(false);
        }
        public static async Task CreatePackJsonAsync(DirectoryInfo rootDir, OldInfoAsmdef asmdef)
        {
            var template = new FileInfo(Config.PathTemplatePackJson);
            if (!template.Exists) throw new FileNotFoundException(template.FullName);

            var newInfo = template.CopyTo(Path.Combine(asmdef.FileInfo!.Directory!.FullName, InfoPackageJson.FILE_NAME));
            string json = await File.ReadAllTextAsync(newInfo.FullName);
            JsonObject jObj = JsonNode.Parse(json)!.AsObject();
            string name = asmdef.FileInfo.Name.ToLowerInvariant();
            jObj[InfoPackageJson.PROP_NAME] = name;
            jObj[InfoPackageJson.PROP_DISPLAY_NAME] = name;
            jObj[InfoPackageJson.PROP_ASMDEF_GUID] = asmdef.Guid;
            await File.WriteAllTextAsync(newInfo.FullName, jObj.ToJsonString(Shared.jOptions));
            Console.WriteLine($"Created package.json:{newInfo.FullName}");
        }

        public static int DirDifferenceLevel(DirectoryInfo root, DirectoryInfo target)
        {
            if (target.FullName.StartsWith(root.FullName))
            {
                var current = target;
                int level = default;

                while (current != null)
                {
                    if (current.FullName == root.FullName)
                    {
                        return level;
                    }
                    else
                    {
                        level++;
                        current = current.Parent;
                    }

                }
                //throw new InvalidOperationException(@"Unexpected behaviour. Probably case: C:\dirname compare to C:\dirname_butlonger");
            }
            return -1;
        }

        public static async Task InsertCsprojToPackSln(DirectoryInfo root, List<InfoBase> infos)
        {
            FileInfo slnFileInfo = new FileInfo(Path.Combine(root.FullName, "packs.sln"));
            InfoSln infoSln = new InfoSln(slnFileInfo);

            foreach (var info in infos)
            {
                if (info is InfoCsproj csproj)
                {
                    infoSln.AddOrUpdateDirty(csproj);
                }
            }
            infoSln.Save();
        }

        public static async Task RenameFolderAsync(DirectoryInfo directoryInfo)
        {
            throw new System.NotImplementedException();
        }

        public static async Task CorrespondAsmdefToCsprojAsync()
        {
            using ModulesDbContextV1 context = new ModulesDbContextV1();
            await context.CorrespondingAsmdefToCsprojAsync().ConfigureAwait(false);
        }

        public static async Task InitUnityPackage(DirectoryInfo directory, string name)
        {
            Console.WriteLine($"InitUnityPackage(); directory:{directory.FullName}; name:{name}; {typeof(IziProjectsOperations).FullName}");
            await InfoCsproj.CreateDefault(directory, name).ConfigureAwait(false);
            await OldInfoAsmdef.CreateDefault(directory, name).ConfigureAwait(false);
            await InfoPackageJson.CreateDefaultAsync(directory, name).ConfigureAwait(false);
            await IziProjectsFinding.CreateDefaultFileAsync(directory).ConfigureAwait(false);
        }

        public static void FixJunctions(string targetPath = "")
        {
            string path = Directory.GetCurrentDirectory();
            if (!string.IsNullOrEmpty(targetPath))
            {
                path = targetPath;
            }

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (var subdir in di.GetDirectories())
            {
                string subdireName = subdir.Name;
                if (!subdir.IsJunction())
                {
                    string newPath = Path.Combine(ConstantsForIziProjects.Paths.LIB_FOLDER, subdireName);
                    Console.WriteLine($"Detected non Juntion: {subdir.FullName}. new path:{newPath}");
                    subdir.MoveTo(newPath);
                    DirectoryInfo newSubdir = new DirectoryInfo(newPath);
                    if (!newSubdir.Exists) throw new DirectoryNotFoundException(newSubdir.FullName);
                    UtilityForMklink.JunctionTargetDirToDir(newSubdir, di);
                    Console.WriteLine($"Moved: {subdir.FullName} to {newSubdir.FullName}");
                }
            }
        }

        /// <summary>
        /// <see cref="IziProjectsOperations.CreatePackJsonAsync(DirectoryInfo, OldInfoAsmdef)"/>
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static async Task CreateNewAsync(DirectoryInfo directory)
        {
            FileInfo? fInfo = directory.GetFiles().FirstOrDefault(x => OldInfoAsmdef.IsValidExtension(x.Extension));

            if (fInfo != null)
            {
                OldInfoAsmdef infoAsmdef = new OldInfoAsmdef(fInfo);
                await IziProjectsOperations.CreatePackJsonAsync(directory, infoAsmdef).ConfigureAwait(false);
            }
            else
            {
                await InfoPackageJson.CreateDefaultAsync(directory).ConfigureAwait(false);
            }
        }
    }
}