using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using IziHardGames.ConsoleArguments;
using IziHardGames.FileSystem.NetStd21;
using static IziHardGames.Projects.DataBase.ConstantsForIziProjects;

namespace IziHardGames.Projects
{
    public class CommandRouterV1
    {
        public const string ARG_NESTED = "nested";
        public const string ARG_UPDATE_FORMAT = "update_format";
        public const string ARG_FORMAT_ASMDEF = "format_asmdef";
        public const string ARG_RESTORE_JUNCTIONS_ASMDEF = "restore_junctions_asmdef";
        public const string ARG_INIT_UNITY_PACKAGE = "init_package_unity";
        public const string ARG_NAME = "name";
        public const string ARG_TARGET = "target";
        public const string ARG_TARGET_PATH = "target_path";

        public static async Task Execute(string[] args)
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
        public static async Task ExecuteAsync(Argument[] arguments)
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
        public static async Task<bool> ExecuteUnariCommandAsync(string[] args)
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
