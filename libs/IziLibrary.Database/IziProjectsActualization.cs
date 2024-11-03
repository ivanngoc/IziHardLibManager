using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;
using IziHardGames.Projects.Sln;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziProjectsActualization
    {
        public static async Task UpdatePackAsync(string pathToAbsSln)
        {
            Console.WriteLine($"Begin {nameof(UpdatePackAsync)}");

            FileInfo fileInfoSln = new FileInfo(pathToAbsSln);
            if (!fileInfoSln.Exists) throw new FileNotFoundException();
            InfoSln infoSln = new InfoSln(fileInfoSln);

            using ModulesDbContextV1 context = new ModulesDbContextV1();
            //var projs = context.Csprojs.Include(x => x.Module).Where(x => x.IsPackProject).ToArray();
            var projs = context.Csprojs.Include(x => x.Module).ToArray();

            foreach (var proj in projs)
            {
                string pathCsproj = proj.PathFull;
                FileInfo csproj = new FileInfo(pathCsproj);
                if (csproj.Exists)
                {
                    await IziProjectsValidations.EnsureSlnToCsprojAsync(fileInfoSln, csproj).ConfigureAwait(false);
                }
                else throw new FileNotFoundException(pathCsproj);
            }
            Console.WriteLine($"Complete {nameof(UpdatePackAsync)}");
        }

        public static async Task UpdateCsprojByUnityAsmdefs()
        {
            Console.WriteLine($"Begin {nameof(UpdateCsprojByUnityAsmdefs)}");

            using ModulesDbContextV1 context = new ModulesDbContextV1();
            var asmdefs = context.UnityAsmdefs.Include(x => x.Module).ToArray();

            foreach (var asmdef in asmdefs)
            {
                var deps = context.GetDependecies(asmdef);
                var csprojFrom = context.GetCorrespondCsproj(asmdef);

                foreach (var item in deps)
                {
                    var csproj = context.GetCorrespondCsproj(item);
                    await EnsureCsprojToCsprojAsync(csprojFrom, csproj).ConfigureAwait(false);
                }
            }
            Console.WriteLine($"Complete {nameof(UpdateCsprojByUnityAsmdefs)}");
        }

        public static async Task UpdateSlnAllDependecie()
        {
            await Config.EnsureLoadedAsync().ConfigureAwait(false);
            await UpdatePackAsync((string)Config.JObj!["sln_all"]!).ConfigureAwait(false);
        }

        public static async Task UpdateFromDir(DirectoryInfo directory)
        {
            var relations = new List<InfoRelation>();
            var result = new List<InfoBase>();
            await IziProjectsFinding.MainSearchAsync(directory, result, 256, relations).ConfigureAwait(false);
            Console.WriteLine($"{nameof(UpdateFromDir)} NotImplemented");
        }

        public static async ValueTask<InfoDll> UpdateInfoDllAsync(DirectoryInfo directory, string fullPath)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            InfoDll infoDll = new InfoDll(fileInfo);
            await infoDll.ExecuteAsync();

            var files = directory.GetFiles();

            foreach (var file in files)
            {
                if (file.Extension.ToLowerInvariant() == ".dll")
                {
                    if (!infoDll.TryGetByFileName(file.Name, out DllRecord record))
                    {
                        record = new DllRecord()
                        {
                            guid = Guid.NewGuid(),
                            filename = file.Name,
                            pathRelative = UtilityForPath.AbsToRelative(directory, file.FullName),
                            pathAbsolute = file.FullName,
                        };
                        infoDll.Add(record);
                        infoDll.IsChanged = true;
                    }
                }
            }
            if (infoDll.IsChanged)
            {
                await File.WriteAllTextAsync(fullPath, infoDll.ToString()).ConfigureAwait(false);
            }
            return infoDll;
        }

        public static async Task EnsureCsprojToCsprojAsync(IziModelCsproj from, IziModelCsproj to)
        {
            FileInfo fileFrom = new FileInfo(from.PathFull);
            FileInfo fileTo = new FileInfo(to.PathFull);

            InfoCsproj projFrom = new InfoCsproj(fileFrom);
            InfoCsproj projTo = new InfoCsproj(fileTo);

            var t1 = projFrom.ExecuteAsync();
            var t2 = projTo.ExecuteAsync();

            await Task.WhenAll(t1, t2).ConfigureAwait(false);

            if (projFrom.EnsureRefToCsproj(projTo))
            {
                projFrom.Proj.Save();
            }
        }
    }
}