using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase;

namespace IziHardGames.Projects
{
    public class InfoPackageJson : InfoBase
    {
        private JsonObject? value;
        private string packageName = string.Empty;
        private string packageVersion = string.Empty;
        private string displayName = string.Empty;
        private string author = string.Empty;

        public JsonObject Value => value ?? throw new NullReferenceException();
        public string PackageName { get => packageName; }
        public string PackageVersion => packageVersion;
        public string DisplayName => displayName;

        public const string FILE_NAME = "package.json";
        public const string FILE_NAME_META = "package.json.meta";

        public const string DEFAULT_ROOT_NAMESAPCE = "IziHardGames";
        public const string DEFAULT_VERSION = "1.0.0";
        public const string DEFAULT_AUTHOR_NAME= "Tran Ngoc Anh";
        public const string DEFAULT_AUTHOR_EMAIL= "tna.work10@gmail.com";
        public const string DEFAULT_AUTHOR_URL= "http://izhg.com/index.php/Unity3d_Modular";

        public const string PROP_ROOT_NAMESPACE = "rootNamespace";
        public const string PROP_NAME = "name";
        public const string PROP_VERSION = "version";
        public const string PROP_DISPLAY_NAME = "displayName";
        public const string PROP_DESCRIPTION = "description";
        public const string PROP_UNITY = "unity";
        public const string PROP_UNITY_RELEASE = "unityRelease";
        public const string PROP_DOCUMENTATION_URL = "documentationUrl";
        public const string PROP_URL_CHANGELOG = "changelogUrl";
        public const string PROP_URL_LICENSE = "licensesUrl";
        public const string PROP_URL_KEYWORDS = "keywords";
        public const string PROP_AUTHOR = "author";
        public const string PROP_AUTHOR_NAME = "name";
        public const string PROP_AUTHOR_EMAIL = "email";
        public const string PROP_AUTHOR_URL = "url";
        public const string PROP_DEPENDENCIES = "dependencies";

        //Cutom fields
        /// <summary>
        /// Guid.ToString("N")
        /// </summary>
        public const string PROP_ASMDEF_GUID = "asmdefGuid";
        /// <summary>
        /// Guid in associated package.json.meta file <br/>
        /// Guid.ToString("D")
        /// </summary>
        public const string PROP_GUID = "guid";

        public InfoPackageJson(FileInfo info) : base(info)
        {

        }

        public override async Task ExecuteAsync()
        {
            this.Content = await File.ReadAllTextAsync(FileInfo!.FullName);
            // как оказалось в  пакетах Unity asmdef может иметь сразу 2 свойства с одни и тем же именем. Тогда парсер падает. так что если и тут упадет из за этого
            /// то надо использовать <see cref="JsonDocument"/> как в <see cref="InfoAsmdef.ExecuteAsync"/>.FromBytes()
            value = JsonNode.Parse(Content)!.AsObject();
            var authorEl = value[PROP_AUTHOR];
            var unity = value[PROP_UNITY];
            var unityRelease = value[PROP_UNITY_RELEASE];

            if (unity == null && unityRelease == null)
            {
                IsSkipped = true;
                return;
            }
            if (authorEl is JsonObject jObj)
            {
                author = (string)authorEl[PROP_AUTHOR_NAME]! ?? author;
            }
            else if (authorEl is JsonValue value)
            {
                author = (string)value!;
            }
            if (!string.Equals(author, "Tran Ngoc Anh", StringComparison.InvariantCultureIgnoreCase) && !string.Equals(author, "IziHardGames", StringComparison.InvariantCultureIgnoreCase))
            {
                IsThirdParty = true;
            }

            displayName = ((string)value[PROP_DISPLAY_NAME]!) ?? throw new NullReferenceException();
            packageName = ((string)value[PROP_NAME]!) ?? throw new NullReferenceException();
            packageVersion = ((string)value[PROP_VERSION]!) ?? throw new NullReferenceException();
            var existedGuid = value[PROP_GUID];

            if (existedGuid != null)
            {
                var guid = (string)existedGuid!;
                if (!string.IsNullOrEmpty(guid))
                {
                    SetGuidFounded(System.Guid.Parse(guid));
                }
            }

            if (!this.IsGuidFounded)
            {
                isGuidGenerated = true;
                SetGuid(global::System.Guid.NewGuid());
                value[PROP_GUID] = this.Guid;
                var json = value.ToJsonString(Shared.jOptions);
                await SaveAsync(json);
            }
            IsExecuted = true;
        }

        public async Task SaveAsync(string json)
        {
            this.Content = json;
            await File.WriteAllTextAsync(FileInfo!.FullName, json);
        }

        public void FindConnections(List<InfoRelation> result, Func<string, string, InfoPackageJson> finder)
        {
            if (!IsExecuted) throw new InvalidOperationException($"You must call {nameof(ExecuteAsync)} before that moment");

            var deps = value![PROP_DEPENDENCIES]?.AsObject();

            if (deps != null)
            {
                foreach (var item in deps)
                {
                    var packageName = item.Key;
                    var version = (string)item.Value!.AsValue()!;
                    if (string.IsNullOrEmpty(version)) throw new FormatException();
                    var package = finder.Invoke(packageName, version);
                    var connection = new InfoRelation()
                    {
                        from = this,
                        to = package,
                        flags = ERelationsFlags.None,
                    };
                    result.Add(connection);
                }
            }
        }

        public string GetPackageName()
        {
            if (!IsExecuted) throw new InvalidOperationException($"You must call {nameof(ExecuteAsync)} before that moment");
            return packageName;
        }

     
        public static async Task CreateDefaultAsync(DirectoryInfo directory, string name = "")
        {
            var template = new FileInfo(Config.PathTemplatePackJson);

            string fullName = Path.Combine(directory.FullName, FILE_NAME);
            string fullNameMeta = Config.PathTemplatePackJsonMeta;

            string json = await File.ReadAllTextAsync(template.FullName);
            JsonObject jObj = JsonNode.Parse(json)!.AsObject();

            Guid guid = System.Guid.NewGuid();

            if (string.IsNullOrEmpty(name))
            {
                name = directory.Name;
            }

            jObj[InfoPackageJson.PROP_NAME] = name;
            jObj[InfoPackageJson.PROP_DISPLAY_NAME] = name;
            jObj[InfoPackageJson.PROP_ASMDEF_GUID] = "";
            jObj[InfoPackageJson.PROP_GUID] = guid.ToString("D");

            await File.WriteAllTextAsync(fullName, jObj.ToJsonString(Shared.jOptions));

            string metaText = await File.ReadAllTextAsync(fullNameMeta);

            if (InfoUnityMeta.OverrideGuid(metaText, guid, out var newText))
            {
                await File.WriteAllTextAsync(fullNameMeta, newText);
            }
            else
            {
                throw new FormatException(metaText);
            }
            Console.WriteLine($"Created package.json:{fullName}");
        }

        public static bool IsValid(FileInfo fileInfo)
        {
            return IsValid(fileInfo.Name);
        }
        public static bool IsValid(string name)
        {
            return string.Equals(FILE_NAME, name, StringComparison.InvariantCultureIgnoreCase);
        }

        public static async ValueTask<Guid> GetGuidAsync(FileInfo fileInfo)
        {
            string text = await File.ReadAllTextAsync(fileInfo.FullName);
            var jobj = JsonNode.Parse(text)!.AsObject();

            if (jobj[PROP_GUID] != null)
            {
                var guid = (string)jobj[PROP_GUID]!;
                if (string.IsNullOrEmpty(guid)) return default;
                return System.Guid.Parse(guid);
            }
            return default;
        }

        public static async Task OverrideGuidAsync(FileInfo fileInfo, Guid toSet)
        {
            string text = await File.ReadAllTextAsync(fileInfo.FullName);
            var jobj = JsonNode.Parse(text)!.AsObject();
            jobj[PROP_GUID] = toSet.ToString("D");
            await File.WriteAllTextAsync(fileInfo.FullName, jobj.ToJsonString(Shared.jOptions));
        }

        public static async Task<bool> IsFromUnityAndMineAsync(FileInfo info)
        {
            var content = await File.ReadAllTextAsync(info!.FullName);
            // как оказалось в  пакетах Unity asmdef может иметь сразу 2 свойства с одни и тем же именем. Тогда парсер падает. так что если и тут упадет из за этого
            /// то надо использовать <see cref="JsonDocument"/> как в <see cref="InfoAsmdef.ExecuteAsync"/>.FromBytes()
            var value = JsonNode.Parse(content)!.AsObject();
            var unity = value[PROP_UNITY];
            var unityRelease = value[PROP_UNITY_RELEASE];
            var author = value[PROP_AUTHOR];
            if (author == null) return false;

            return unity != null && unityRelease != null && string.Equals((string)(author[PROP_AUTHOR_NAME] ?? string.Empty)!, ConstantsForIziProjects.VALUE_AUTHOR, StringComparison.InvariantCultureIgnoreCase);
        }
        public static async Task<bool> IsFromUnityAsync(FileInfo info)
        {
            var content = await File.ReadAllTextAsync(info!.FullName);
            // как оказалось в  пакетах Unity asmdef может иметь сразу 2 свойства с одни и тем же именем. Тогда парсер падает. так что если и тут упадет из за этого
            /// то надо использовать <see cref="JsonDocument"/> как в <see cref="InfoAsmdef.ExecuteAsync"/>.FromBytes()
            var value = JsonNode.Parse(content)!.AsObject();
            var unity = value[PROP_UNITY];
            var unityRelease = value[PROP_UNITY_RELEASE];

            if (unity == null && unityRelease == null)
            {
                return false;
            }
            return true;
        }

        public static async Task<FileInfo?> FindMineAsync(DirectoryInfo dirAsmdef)
        {
            DirectoryInfo? dir = dirAsmdef;

            while (dir != null)
            {
                var files = dir.GetFiles().Where(x => InfoPackageJson.IsValid(x));

                foreach (var file in files)
                {
                    Console.WriteLine($"File:{file.FullName}. {typeof(InfoPackageJson).FullName}");
                    if (await IsFromUnityAndMineAsync(file))
                    {
                        return file;
                    }
                }
                dir = dir.Parent;
            }
            return null;
        }
    }
}