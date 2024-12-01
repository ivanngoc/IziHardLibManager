using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Metas.Models;

namespace IziHardGames.Asmdefs.Models
{

    public class Asmdef
    {
        public const string CUSTOM_PROP_GUID = "UNITY_META_GUID";

        private readonly FileInfo fi;

        public Asmdef(FileInfo fi)
        {
            this.fi = fi;
        }

        public async Task<AsmdefId?> GetGuidAsync()
        {
            var json = JsonObject.Parse(await File.ReadAllTextAsync(fi.FullName));
            var guidProp = json?[CUSTOM_PROP_GUID];
            if (guidProp != null)
            {
                var id = guidProp.GetValue<Guid>();
                if (id != Guid.Empty)
                {
                    return new AsmdefId(id);
                }
            }

            var meta = Meta.FromAsmdef(fi);
            if (meta != null)
            {
                var guid = await meta.GetGuidAsmdefAsync();
                if (guid.HasValue)
                {
                    return new AsmdefId(guid.Value);
                }
            }
            return null;
        }
    }
}
