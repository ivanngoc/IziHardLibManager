using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase.Models;
using IziHardGames.Projects.Sln;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects
{
    public static class IziProjectsValidations
    {
        public static async Task FixAsmdefGuids(DirectoryInfo root)
        {
            throw new System.NotImplementedException();
        }
        public static async Task FixNestedPackageJson(DirectoryInfo root)
        {
            var dirs = root.GetDirectories();

            foreach (var dir in dirs)
            {
                var packJsons = new List<FileInfo>();
                var count = await dir.CountNestedAsync(async x =>
                {
                    if (x.IsJunction()) return false;
                    var files = x.GetFiles().Where(x => InfoPackageJson.IsValid(x));
                    bool isFounded = false;
                    foreach (var file in files)
                    {
                        if (await InfoPackageJson.IsFromUnityAndMineAsync(file))
                        {
                            packJsons.Add(file);
                            isFounded = true;
                        }
                    }
                    return isFounded;
                });
                if (count > 1)
                {
                    Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    // each file has name package.json 
                    var sorted = packJsons.OrderBy(x => x.FullName.Length);
                    foreach (var item in sorted)
                    {
                        Console.WriteLine(item.FullName);
                    }
                    Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                }
            }
            throw new System.NotImplementedException("Пока не решил как сделать. NestedCount() суммирует результат для параллеьных папок! если в root не было package.json а в подпапках были то результат будет например 2 при том что это 2 ветки которые стартуют на 1 уровень позже");
        }

        public static async Task FixPackageName(List<InfoBase> result)
        {
            int i = 0;
            foreach (var item in result)
            {
                if (item is InfoPackageJson info)
                {
                    var newInfo = item.FileInfo!.Directory!.GetFiles().First(x => x.Name == InfoPackageJson.FILE_NAME);
                    string json = await File.ReadAllTextAsync(newInfo.FullName);
                    JsonObject jObj = JsonNode.Parse(json)!.AsObject();
                    var existedName = (string)jObj["name"]!;
                    if (existedName == InfoPackageJson.FILE_NAME)
                    {
                        i++;
                        var file = info.FileInfo!.Directory!.GetFiles().FirstOrDefault(x => x.Extension == ".csproj");
                        string name = $"no_asm_def_{i}";
                        if (file != null)
                        {
                            name = file.Name.ToLowerInvariant();
                        }
                        jObj["name"] = name;
                        jObj["displayName"] = name;
                        jObj["dependencies"] = null;
                        await File.WriteAllTextAsync(newInfo.FullName, jObj.ToJsonString(Shared.jOptions));
                        Console.WriteLine($"Fixed package.json:{newInfo.FullName}");
                    }
                }
            }
        }
        ///
        internal static async Task EnsureCsprojToCsprojAsync(IziModelCsproj from, IziModelCsproj to)
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
        public static async Task EnsureSlnToCsprojAsync(FileInfo sln, FileInfo csproj)
        {
            await SlnMappedFile.EnsureCsprojReferencedAsync(sln, csproj);
        }

        public static async Task EnsureRoots(DirectoryInfo directory)
        {
            throw new System.NotImplementedException();
        }

        internal static void EnsureGuid(List<InfoBase> list)
        {
            foreach (var item in list)
            {
                if (item.GuidStruct == default || string.IsNullOrEmpty(item.Guid))
                {
                    throw new FormatException($"No Guid. {item.GetType().FullName}");
                }
            }
        }
    }
}