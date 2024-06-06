using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziHardGames.ConsoleArguments;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;
using IziHardGames.Projects.Sln;
using CommandLine;
using static IziHardGames.Projects.DataBase.ConstantsForIziProjects;

namespace IziHardGames.Projects
{
    public static class Program
    {
        public const string ARG_NESTED = "nested";
        public const string ARG_UPDATE_FORMAT = "update_format";
        public const string ARG_FORMAT_ASMDEF = "format_asmdef";
        public const string ARG_RESTORE_JUNCTIONS_ASMDEF = "restore_junctions_asmdef";
        public const string ARG_INIT_UNITY_PACKAGE = "init_package_unity";
        public const string ARG_NAME = "name";
        public const string ARG_TARGET = "target";
        public const string ARG_TARGET_PATH = "target_path";
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Version:{ArgumentsReader.MAGIC}");

            if (await ExecuteUnariCommandAsync(args))
            {

            }
            else if (args.Length == 1)
            {
                if (Guid.TryParse(args[0], out var guid))
                {
                    using ModulesDbContext context = new ModulesDbContext();
                    var module = context.Modules.FirstOrDefault(x => x.Guid == guid);
                    if (module != null)
                    {
                        Console.WriteLine($"Name:{module.Name}  Description:{module.Description}. EModuleType:{(EModuleType)module.Type}");

                        var csproj = context.Csprojs.FirstOrDefault(x => x.Module == module);
                        var asmdef = context.UnityAsmdefs.FirstOrDefault(x => x.Module == module);
                        if (csproj != null)
                        {
                            Console.WriteLine($"File:   {csproj.PathFull}");
                        }
                        if (asmdef != null)
                        {
                            Console.WriteLine($"File:   {asmdef.PathFull}; isThirdPart:{asmdef.IsThirdParty}");
                        }
                    }
                    Console.ReadLine();
                    return;
                }
            }
            else
            {


                var dir = new DirectoryInfo("C:\\.izhg-lib");
                //var dirs = dir.GetDirectories().Where(x => x.Name.StartsWith("com."));
                //foreach (var item in dirs)
                //{
                //    await IziEnsurePackageJson.EnsurePackageJson(item);
                //}

                //await Execute(new string[] { "--restore_junctions_asmdef=\"\"", "--target_path=\"\\\"" });
                //await IziProjectsFormatters.FormatJsonPackagesAsync(dir).ConfigureAwait(false);
                //await IziProjectsFormatters.FormatAsmdefsNestedAsync(dir).ConfigureAwait(false);

                if (args.Length != 0)
                {
                    //DirectoryInfo info = new DirectoryInfo(@"C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Assets\\[Project] GameProject3\\Scripts");
                    //await IziProjectsValidations.FormatProjectDir(info);
                    //Console.ReadKey();
                    await Execute(args);
                }
                else
                {
                    if (false) await IziProjectsFinding.FindAllFromConfig();
                    if (false) await SlnMappedFile.Test1();
                    if (false) await SlnMappedFile.Test0();
                    if (false) await IziProjectsFormatters.FormatJsonPackagesAsync(new DirectoryInfo("C:\\.izhg-lib"));
                    if (false) await IziEnsureSlnPack.EnsureDependecies();
                    if (false) await IziEnsurePackageJson.EnsureDependeciesInPackageJson();
                    if (false) await IziProjectsValidations.FixNestedPackageJson(new DirectoryInfo("C:\\.izhg-lib"));

                    if (false) await IziEnsurePackageJson.EnsurePackageJsonAuthorMe(new DirectoryInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages"));
                    if (false) await IziEnsureCsproj.RegenerateByTemplate(new DirectoryInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages"));
                    if (false) IziEnsureCsproj.EnsureAuthorIsMe(new DirectoryInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages"));
                    if (false) IziEnsureCsproj.EnsureProjectName(new DirectoryInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages"));

                    if (true) await ModulesDbContext.InitAsync();
                    if (true) await IziUnityCache.InitilizeAsync();

                    if (false) await IziEnsureCsproj.EnsureGuidInDataBase();
                    if (false) await IziEnsureAsmdef.EnsureReferencesToUnityDll();
                    if (false) await IziEnsureAsmdef.EnsureAsmdefDependeciesInCsproj();
                    if (false) await IziEnsureCsproj.EnsureProjectReferenceMetaGuid();
                    if (false) await IziProjectsActualization.UpdateSlnAllDependecie();

                    if (false) await IziProjectsActualization.UpdateCsprojByUnityAsmdefs();
                    if (false) await IziProjectsActualization.UpdatePackAsync(null);
                    if (false) await IziProjectsOperations.CorrespondAsmdefToCsprojAsync();




                    if (false) await InfoCsproj.TestAsync();
                    if (false) await InfoCsproj.TestAsync();
                    if (false) await InfoCsproj.TestAsync3();

                    if (false)
                    {
                        CollectInfo ci = new CollectInfo();
                        await ci.ExecuteAsync();
                    }
                }
            }
            Console.WriteLine("ProgramEND");
            Console.ReadLine();
        }

        private static async Task Execute(string[] args)
        {
            Console.WriteLine($"Started IziProjectsManager. Dir: {Directory.GetCurrentDirectory()}");

            if (args.Length > 0)
            {
                Console.WriteLine("Args:");
                Console.WriteLine(args.Aggregate((x, y) => x + Environment.NewLine + y));
                ArgumentsReader argumentsBinder = new ArgumentsReader();
                argumentsBinder.UseDefaultVerbose();
                argumentsBinder.Regist(DefaultPath.Path);
                argumentsBinder.Regist('u', "update");
                argumentsBinder.Regist('e', "ensure"); /// ensure <see cref="InfoIziProjectsMeta"/>
                argumentsBinder.Regist('d', "dependecies"); /// update dependecies
                argumentsBinder.Regist('a', "database"); /// update database
                argumentsBinder.Regist('l', "dll");
                argumentsBinder.Regist('t', ARG_UPDATE_FORMAT);
                argumentsBinder.Regist('f', ARG_FORMAT_ASMDEF);
                argumentsBinder.Regist('n', ARG_NESTED);
                argumentsBinder.Regist(Argument.WILDCARD, ARG_RESTORE_JUNCTIONS_ASMDEF);
                argumentsBinder.Regist(Argument.WILDCARD, ARG_INIT_UNITY_PACKAGE);
                argumentsBinder.Regist(Argument.WILDCARD, ARG_NAME);
                argumentsBinder.Regist(Argument.WILDCARD, ARG_TARGET);
                argumentsBinder.Regist(Argument.WILDCARD, ARG_TARGET_PATH);

                argumentsBinder.Parse(args);
                Console.WriteLine("String Info");
                Console.WriteLine(argumentsBinder.ToStringInfo());
                await ExecuteAsync(argumentsBinder.GetFiredArguments()).ConfigureAwait(false);
                return;
            }
            else
            {
                Console.WriteLine("No Args");
            }

            Console.WriteLine("Ended");
        }
        private static async Task ExecuteAsync(Argument[] arguments)
        {
            DirectoryInfo? directory = null;
            DirectoryInfo? targetDir = null;
            string path = Directory.GetCurrentDirectory();

            string name = string.Empty;
            var nameArg = arguments.FirstOrDefault(x => x.prefixFull == ARG_NAME);

            if (nameArg != null)
            {
                name = nameArg.argRecieved;
            }

            if (arguments.TryFindFirst(x => x.prefixFull == "path", out var argPath))
            {
                string pathArg = argPath.argRecieved;
                if (UtilityForPath.IsRelative(pathArg))
                {
                    directory = new DirectoryInfo(path);
                    path = UtilityForPath.Combine(directory, pathArg, UtilityForPath.GetSeparator(path));
                }
                else
                {
                    if (UtilityForPath.IsValidPath(pathArg, out directory))
                    {
                        path = directory.FullName;
                    }
                    else
                        throw new System.NotImplementedException($"Directory doesn't exists: {pathArg}");
                }
            }
            if (directory == null) directory = new DirectoryInfo(path);

            string targetPath = string.Empty;
            string targetPathAbs = string.Empty;
            var targPathArg = arguments.FirstOrDefault(x => x.prefixFull == ARG_TARGET_PATH);

            if (targPathArg != null)
            {
                targetPath = targPathArg.argRecieved;
                if (UtilityForPath.IsRelative(targetPath))
                {
                    targetPathAbs = UtilityForPath.Combine(directory, targetPath, Path.DirectorySeparatorChar);
                }
                else
                {
                    targetPathAbs = targetPath;
                }
                if (!Directory.Exists(targetPathAbs))
                {
                    throw new DirectoryNotFoundException(targetPathAbs);
                }
                targetDir = new DirectoryInfo(targetPathAbs);
            }

            if (arguments.Any(x => x.prefixFull == "ensure"))
            {
                Console.WriteLine($"IziMetaAsync()");
                var meta = await IziEnsure.IziMetaAsync(directory).ConfigureAwait(false);
            }
            if (arguments.Any(x => x.prefixFull == "update"))
            {
                Console.WriteLine($"UpdateFromDir()");
                await IziProjectsActualization.UpdateFromDir(directory).ConfigureAwait(false);
            }
            if (arguments.TryFindFirst(x => x.prefixFull == "dll", out var argDll))
            {
                Console.WriteLine($"IziMetaDllJson()");
                await IziEnsure.IziMetaDllJson(directory).ConfigureAwait(false);
            }
            if (arguments.Any(x => x.prefixFull == ARG_UPDATE_FORMAT))
            {
                Console.WriteLine($"{nameof(IziProjectsValidations)}");
                await IziProjectsFormatters.FormatProjectDir(directory).ConfigureAwait(false);
            }

            if (arguments.Any(x => x.prefixFull == ARG_FORMAT_ASMDEF))
            {
                if (arguments.Any(x => x.prefixFull == ARG_NESTED))
                {
                    await IziProjectsFormatters.FormatAsmdefsNestedAsync(directory).ConfigureAwait(false);
                }
                else
                {
                    await IziProjectsFormatters.FormatAsmdefsAsync(directory).ConfigureAwait(false);
                }
            }
            if (arguments.Any(x => x.prefixFull == ARG_RESTORE_JUNCTIONS_ASMDEF) && arguments.Any(x => x.prefixFull == ARG_TARGET))
            {
                var targetAsmdef = arguments.First(x => x.prefixFull == ARG_TARGET);
                await IziEnsureAsmdef.EnsureDependeciesJunctionsAsync(targetAsmdef.argRecieved).ConfigureAwait(false);
            }
            else if (arguments.Any(x => x.prefixFull == ARG_RESTORE_JUNCTIONS_ASMDEF) && targetPath != null && targetDir != null)
            {
                await IziEnsureAsmdef.EnsureDependeciesJunctionsAsync(directory, targetDir).ConfigureAwait(false);
            }

            if (arguments.Any(x => x.prefixFull == ARG_INIT_UNITY_PACKAGE) && nameArg != null)
            {
                await IziProjectsOperations.InitUnityPackage(directory, name).ConfigureAwait(false);
            }
        }

        private static async Task<bool> ExecuteUnariCommandAsync(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0][0] == '-') return false;
                if (args[0] == UnariCommands.FIX_JUNCT)
                {
                    IziProjectsOperations.FixJunctions();
                    return true;
                }
                else
                {
                    throw new NotImplementedException(args[0]);
                }
            }
            else return false;
        }
    }
}
