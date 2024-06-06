using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static IziHardGames.Projects.DataBase.ConstantsForIziProjects;

namespace IziHardGames.Projects
{
    public static class Config
    {
        private static string pathTemplatePackJson = string.Empty;
        private static string pathTemplatePackJsonMeta = string.Empty;
        private static string pathTemplateCsproj = string.Empty;
        private static string pathTemplateAsmdef = string.Empty;
        private static string pathTemplateAsmdefMeta = string.Empty;

        private static string[] dirsUnityCaches = Array.Empty<string>();
        public static string[] DirsUnityCaches { get { EnsureLoaded(); return dirsUnityCaches; } set => dirsUnityCaches = value; }
        public static string PathTemplatePackJson { get { EnsureLoaded(); return pathTemplatePackJson; } set => pathTemplatePackJson = value; }
        public static string PathTemplatePackJsonMeta { get { EnsureLoaded(); return pathTemplatePackJsonMeta; } set => pathTemplatePackJsonMeta = value; }
        public static string PathTemplateCsproj { get { EnsureLoaded(); return pathTemplateCsproj; } set => pathTemplateCsproj = value; }
        public static string PathTemplateAsmdef { get { EnsureLoaded(); return pathTemplateAsmdef; } set => pathTemplateAsmdef = value; }
        public static string PathTemplateAsmdefMeta { get { EnsureLoaded(); return pathTemplateAsmdefMeta; } set => pathTemplateAsmdefMeta = value; }
        public static string PathPack { get; set; } = string.Empty;
        public static string PathPackSln { get; set; } = string.Empty;
        public static DirectoryInfo? Root { get; set; }
        public static JsonObject? JObj { get; set; }
        public static bool IsLoaded { get; private set; }
        public static IEnumerable<string> SearchDirs => dirs;
        private readonly static List<string> dirs = new List<string>();
        private readonly static List<string> excludes = new List<string>();
        public const string PROP_SCAN_DIR_CSPROJ = "scan_dirs_csproj_lib";
        public const string PROP_SCAN_DIRS = "scan_dirs";
        public const string PROP_SCAN_EXCLUDE = "scan_exclude";
        public const string PATH_DEFAULT = "config.json";
        public const string GLOBAL_VARIABLE = "IZHG_PROJECT_MANAGER_CONFIG";


        public static void Create(string relativePathConfig)
        {
            throw new NotImplementedException();
        }
        public static string GetConfigPath()
        {
            return Environment.GetEnvironmentVariable(GLOBAL_VARIABLE) ?? throw new NullReferenceException($"Environment variable with path to config is not defined. VAR name:{GLOBAL_VARIABLE}");
        }
        public static Task LoadAsync()
        {
            return LoadAsync(GetConfigPath());
        }
        public static async Task LoadAsync(string relativePathConfig)
        {
            var text = await File.ReadAllTextAsync(relativePathConfig);
            var config = JsonNode.Parse(text)!.AsObject();
            JObj = config;
            PathPack = (string)config["path_pack"]!;
            PathPackSln = (string)config["pack_sln"]!;
            var templatesPaths = config["templates"]!;
            PathTemplatePackJson = (string)(templatesPaths[Templates.PACK_JSON_UNITY] ?? throw new FormatException())!;
            PathTemplatePackJsonMeta = (string)(templatesPaths[Templates.PACK_JSON_UNITY_META] ?? throw new FormatException())!;
            PathTemplateCsproj = (string)(templatesPaths[Templates.CSPROJ] ?? throw new FormatException())!;
            PathTemplateAsmdef = (string)(templatesPaths[Templates.ASMDEF] ?? throw new FormatException())!;
            PathTemplateAsmdefMeta = (string)(templatesPaths[Templates.ASMDEF_META] ?? throw new FormatException())!;
            DirsUnityCaches = config!["scan"]!["unity_packages_caches"]!.AsArray().Select(x => (string)x!)!.ToArray();
            Root = new DirectoryInfo(PathPack);
            IsLoaded = true;
            dirs.Clear();
            dirs.AddRange(config[PROP_SCAN_DIR_CSPROJ]!.AsArray()!.Select(x => (string)x!).Where(x => !string.IsNullOrEmpty(x)));
            dirs.AddRange(config[PROP_SCAN_DIRS]!.AsArray()!.Select(x => (string)x!).Where(x => !string.IsNullOrEmpty(x)));
            excludes.AddRange(config[PROP_SCAN_EXCLUDE]!.AsArray()!.Select(x => (string)x!).Where(x => !string.IsNullOrEmpty(x)));
        }

        public static void EnsureLoaded()
        {
            var t1 = EnsureLoadedAsync();
            t1.Wait();
        }
        public static async Task EnsureLoadedAsync()
        {
            if (IsLoaded)
            {
                return;
            }
            await LoadAsync(GetConfigPath());
        }
    }
}
