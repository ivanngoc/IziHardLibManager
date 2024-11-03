using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziHardGames.ConsoleArguments;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.Sln;

namespace IziHardGames.Projects
{
    public static partial class Program
    {

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Version:{ArgumentsReader.MAGIC}");

            Core core = new Core();
            var provider = await core.Run().ConfigureAwait(false);

            if (await CommandRouterV1.ExecuteUnariCommandAsync(args))
            {

            }
            else if (args.Length == 1)
            {
                if (Guid.TryParse(args[0], out var guid))
                {
                    using ModulesDbContextV1 context = new ModulesDbContextV1();
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
                    await CommandRouterV1.Execute(args);
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

                    if (true) await ModulesDbContextV1.InitAsync();
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
    }
}
