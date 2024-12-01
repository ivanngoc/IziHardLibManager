using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

namespace IziHardGames.Metas.Models
{
    public class Meta
    {
        private readonly FileInfo fiMeta;
        public Meta(FileInfo fiMeta)
        {
            this.fiMeta = fiMeta;
        }

        public static Meta? FromAsmdef(FileInfo fi)
        {
            var name = fi.FullName + ".meta";
            var fiMeta = new FileInfo(name);
            if (fiMeta.Exists)
            {
                return new Meta(fiMeta);
            }
            return null;
        }

        public async Task<Guid?> GetGuidAsmdefAsync()
        {
            var content = await File.ReadAllTextAsync(fiMeta.FullName);
            var v = YamlConverter.Deserialize<ExpandoObject>(content);
            var jsonString = JsonSerializer.Serialize(v);
            var jObj = JsonObject.Parse(jsonString);
            var guidProp = jObj?["guid"];
            if (guidProp != null)
            {
                var guidAsStr = guidProp.GetValue<string>();
                var guid = Guid.Parse(guidAsStr);
                if (guid != Guid.Empty)
                {
                    return guid;
                }
            }
            return null;
        }
    }
}
