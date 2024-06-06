using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziEnsurePackageJson
    {
        public static async Task EnsurePackageJsonAsync(DirectoryInfo dir)
        {
            FileInfo? existed = dir.GetFiles().FirstOrDefault(x => InfoPackageJson.IsValid(x.Name));
            if (existed == null)
            {
                await IziProjectsOperations.CreatePackJsonAsync(dir);
            }
        }
        /// <summary>
        /// <see cref="InfoPackageJson.author"/>
        /// </summary>
        /// <returns></returns>
        public static async Task EnsurePackageJsonAuthorMe(DirectoryInfo dir)
        {
            List<FileInfo> files = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(dir, files, (x) => x.Name == InfoPackageJson.FILE_NAME);

            foreach (FileInfo file in files)
            {
                string s = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);
                var jObj = JsonNode.Parse(s)!.AsObject();
                jObj[InfoPackageJson.PROP_AUTHOR]![InfoPackageJson.PROP_AUTHOR_NAME] = "Tran Ngoc Anh";
                await File.WriteAllBytesAsync(file.FullName, Encoding.UTF8.GetBytes(jObj.ToJsonString(Shared.jOptions)));
            }
        }

        /// <summary>
        /// Гарант-проверка свойства dependecies внутри файла package.json (пакета Unity3d) на корректность ссылок на зависимые пакеты
        /// </summary>
        /// <returns></returns>
        public static async Task EnsureDependeciesInPackageJson()
        {
            using ModulesDbContext context = new ModulesDbContext();
            var jsons = context.UnityPackageJsons.Include(x => x.Module).ToArray();

            foreach (var json in jsons)
            {
                var fi = new FileInfo(json.PathFull);
                if (fi.Exists)
                {
                    if (await InfoPackageJson.IsFromUnityAndMineAsync(fi))
                    {
                        await EnsureDependecies(fi, context);
                    }
                }
            }
        }
        public static async Task EnsureDependecies(FileInfo fiPackageJson, ModulesDbContext context)
        {
            var files = fiPackageJson.Directory!.SelectAllFilesBeneathExceptLinksDir().Where(x => InfoAsmdef.IsValidExtension(x));
            var pjsonTarget = new InfoPackageJson(fiPackageJson);
            await pjsonTarget.ExecuteAsync().ConfigureAwait(false);
            var json = pjsonTarget.Value;
            var jsonDeps = new JsonObject();

            foreach (var file in files)
            {
                InfoAsmdef asmdef = new InfoAsmdef(file);
                await asmdef.EnsureExecuted().ConfigureAwait(false);

                foreach (var item in asmdef.Refs)
                {
                    Guid guid = Guid.Parse(item);
                    var depAsmdef = context.UnityAsmdefs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == guid);
                    if (depAsmdef != null)
                    {
                        var fiDepAsmdef = new FileInfo(depAsmdef.PathFull);
                        if (fiDepAsmdef.Exists)
                        {
                            var fiPackJsonDep = IziProjectsFinding.FindMainPackageJson(fiDepAsmdef.Directory!);
                            if (fiPackJsonDep != null)
                            {
                                var packageJsonDep = new InfoPackageJson(fiPackJsonDep);
                                await packageJsonDep.ExecuteAsync().ConfigureAwait(false);
                                if (packageJsonDep.GuidStruct == pjsonTarget.GuidStruct) continue;
                                if (packageJsonDep.IsSkipped) throw new NotImplementedException();
                                var version = packageJsonDep.PackageVersion;
                                var displayName = packageJsonDep.DisplayName;
                                var name = packageJsonDep.PackageName;
                                if (string.IsNullOrEmpty(version)) throw new FormatException($"{fiDepAsmdef.FullName}. version is empty");
                                if (string.IsNullOrEmpty(name)) throw new FormatException($"{fiDepAsmdef.FullName}. version is empty");
                                jsonDeps[name] = version;
                            }
                            else
                            {
                                throw new FileNotFoundException($"Can't find root package.json starting at dir:{fiDepAsmdef.Directory!.FullName}");
                            }
                        }
                        else
                        {
                            throw new FileNotFoundException(fiDepAsmdef.FullName);
                        }
                    }
                    else
                    {
                        throw new NullReferenceException($"Can't find asmdef with Guid:{guid.ToString("D")} in database");
                    }
                }
            }
            json[InfoPackageJson.PROP_DEPENDENCIES] = jsonDeps;
            await File.WriteAllTextAsync(fiPackageJson.FullName, json.ToJsonString(Shared.jOptions)).ConfigureAwait(false);
        }
    }
}