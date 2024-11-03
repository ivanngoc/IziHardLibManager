using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using IziHardGames.IziLibrary.Metas.ForAsmdef;

namespace IziHardGames.Projects
{
    /// <summary>
    /// 
    /// </summary>
    public class OldInfoAsmdef : InfoBase
    {
        public const string EXTENSION = MetaForAsmdef.EXTENSION_ASMDEF;
        /// <summary>
        /// Guid.ToString("D")
        /// </summary>
        public const string PROP_GUID = "izhg_guid";
        public const string PROP_NAME = "name";
        /// <summary>
        /// Мое дополнительное поле в файле .asmdef для сопоставления guid c именем проекта в человеко читаемом формате
        /// </summary>
        public const string PROP_REF_HRF = "references_hrf";
        public const string PROP_REFS = "references";
        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };

        public InfoPackageJson? infoPack;
        public InfoCsproj? infoProj;
        public InfoUnityMeta? infoMeta;
        public bool IsNoUnityEngineRefs { get; set; }
        public bool isIzhgGuidPresented;
        public string[] Refs { get; set; } = Array.Empty<string>();

        public OldInfoAsmdef(FileInfo fileInfo) : base(fileInfo)
        {
            targetExtension = EXTENSION;
        }

        public override async Task ExecuteAsync()
        {
            var fileInfo = this.info;
            Console.WriteLine($"Founded asmdef: {fileInfo!.FullName}");
            var metaAbsPath = fileInfo.FullName + ".meta";
            FileInfo fileInfoMeta = new FileInfo(metaAbsPath);

            if (fileInfoMeta.Exists)
            {
                var meta = this.infoMeta = new InfoUnityMeta(fileInfoMeta, this);
                Console.WriteLine($"Meta exists: {fileInfoMeta.FullName}");
                await meta.ExecuteAsync().ConfigureAwait(false);
                SetGuidFounded(meta.GuidStruct);
            }

            var bytes = await File.ReadAllBytesAsync(fileInfo.FullName);
            var text = Encoding.UTF8.GetString(bytes);
            this.Content = text;
            FromBytes(bytes);
        }

        private void FromBytes(byte[] bytes)
        {
            // some files conteins UTF-8 BOM start symbol
            var span = bytes.AsMemory().Span;
            if (span.StartsWith(Utf8Bom))
            {
                span = span.Slice(Utf8Bom.Length);
            }
            Utf8JsonReader reader = new Utf8JsonReader(span);

            if (JsonDocument.TryParseValue(ref reader, out var doc))
            {
                if (doc.RootElement.TryGetProperty("references", out JsonElement jsonResfs))
                {
                    var refs = JsonArray.Create(jsonResfs)!;
                    Refs = refs.Select(x => ((string)x!)).Where(x => x.StartsWith("GUID", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Split(':')[1].Trim()).ToArray();

                }
                if (doc.RootElement.TryGetProperty("noEngineReferences", out JsonElement element))
                {
                    IsNoUnityEngineRefs = element.GetBoolean();
                }
                if (doc.RootElement.TryGetProperty(PROP_GUID, out JsonElement elGuid))
                {
                    var value = elGuid.GetString() ?? string.Empty;
                    if (System.Guid.TryParse(value, out var guid))
                    {
                        SetGuidFounded(guid);
                        isIzhgGuidPresented = true;
                    }
                }
            }
        }
        private void Parse(string json)
        {
            var jObj = JsonNode.Parse(json)!.AsObject();

            if (jObj.TryGetPropertyValue("noEngineReferences", out var node))
            {
                IsNoUnityEngineRefs = (bool)node!;
                var refs = jObj["references"]!.AsArray();
                Refs = refs.Select(x => ((string)x!)).Where(x => x.StartsWith("GUID", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Split(':')[1].Trim()).ToArray();
            }
        }

        public async ValueTask<string[]> FindReferencesAsync()
        {
            string json = await File.ReadAllTextAsync(FileInfo!.FullName);
            var jObj = JsonNode.Parse(json)!.AsObject();
            var refs = jObj["references"]!.AsArray()!;
            return refs.Select(x => ((string)x!).Split(':')[1].Trim()).ToArray();
        }

        public void SetPairCsproj(InfoCsproj proj)
        {
            this.infoProj = proj;
            SetPaired(proj);
        }

        public bool IsUse(OldInfoAsmdef asmdef)
        {
            return Refs.Any(x => x.ToLowerInvariant().Trim() == asmdef.guid.ToLower().Trim());
        }

        /// <summary>
        /// Только мои asmdef
        /// </summary>
        /// <param name="result"></param>
        /// <param name="findByGuid"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void FindConnectionsOwn(List<InfoRelation> result, Func<string, OldInfoAsmdef> findByGuid)
        {
            if (IsExecuted)
            {
                foreach (var item in Refs)
                {
                    var asmdef = findByGuid(item);
                    var connection = new InfoRelation()
                    {
                        flags = ERelationsFlags.None,
                        from = this,
                        to = asmdef,
                    };
                    result.Add(connection);
                }
            }
        }

        public static bool IsValidExtension(FileInfo info)
        {
            return IsValidExtension(info.Extension);
        }
        public static bool IsValidExtension(string extension)
        {
            return string.Equals(EXTENSION, extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task WriteGuidToFileAsync(Guid guidStruct)
        {
            this.Content = await File.ReadAllTextAsync(FileInfo.FullName).ConfigureAwait(false);
            var jObj = JsonNode.Parse(Content)!.AsObject();
            jObj[PROP_GUID] = guidStruct.ToString("D");
            this.Content = jObj.ToJsonString(Shared.jOptions);
            await File.WriteAllTextAsync(FileInfo.FullName, Content);
        }

        public static Guid FindGuid(JsonNode jsonNode)
        {
            if (jsonNode == null) throw new NullReferenceException();
            var line = (string)jsonNode!;
            var splits = line.Split(':');
            return System.Guid.Parse(splits[1]);
        }

        public static async Task CreateDefault(DirectoryInfo directory, string name)
        {
            var pathTemplate = Config.PathTemplateAsmdef;
            string pathNew = Path.Combine(directory.FullName, name + EXTENSION);
            File.Copy(pathTemplate, pathNew, false);
            var fi = new FileInfo(pathNew);
            var guid = System.Guid.NewGuid();
            await SetGuidAsync(fi, guid).ConfigureAwait(false);
            await SetNameAsync(fi, name).ConfigureAwait(false);

            var pathNewMeta = pathNew + InfoUnityMeta.EXTENSION;
            var fiMeta = new FileInfo(pathNewMeta);
            var pathTemplateMeta = Config.PathTemplateAsmdefMeta;
            File.Copy(pathTemplateMeta, pathNewMeta, false);
            await InfoUnityMeta.SetGuidAsync(fiMeta, guid).ConfigureAwait(false);
        }
        public static async Task SetNameAsync(FileInfo fi, string name)
        {
            var text = await File.ReadAllTextAsync(fi.FullName).ConfigureAwait(false);
            var jobj = JsonNode.Parse(text)!.AsObject();
            jobj[PROP_NAME] = name;
            await File.WriteAllTextAsync(fi.FullName, jobj.ToJsonString(Shared.jOptions)).ConfigureAwait(false);
        }
        public static async Task SetGuidAsync(FileInfo fi, Guid guid)
        {
            var text = await File.ReadAllTextAsync(fi.FullName).ConfigureAwait(false);
            var jobj = JsonNode.Parse(text)!.AsObject();
            jobj[PROP_GUID] = guid.ToString("D");
            await File.WriteAllTextAsync(fi.FullName, jobj.ToJsonString(Shared.jOptions)).ConfigureAwait(false);
        }

        public static ValueTask<Guid> FindGuidFromMetaAsync(FileInfo info)
        {
            FileInfo meta = new FileInfo(info.FullName + InfoUnityMeta.EXTENSION);
            if (meta.Exists)
            {
                return InfoUnityMeta.GetGuidAsync(meta);
            }
            return default;
        }
    }
}