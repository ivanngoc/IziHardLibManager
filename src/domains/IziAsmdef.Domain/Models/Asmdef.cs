using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Metas.Models;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Asmdefs.Models
{

    public class Asmdef
    {
        public const string CUSTOM_PROP_GUID = "UNITY_META_GUID";
        public const string CUSTOM_PROP_TAGS = "UNITY_META_TAGS";

        private readonly FileInfo fi;

        public Asmdef(FileInfo fi)
        {
            this.fi = fi;
        }

        public async Task<AsmdefId?> GetGuidAsync()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async IAsyncEnumerable<EntityTag> GetOrCreateTags(DbSet<EntityTag> context, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var json = JsonObject.Parse(await File.ReadAllTextAsync(fi.FullName));
            var tagsProp = json?[CUSTOM_PROP_TAGS];

            if (tagsProp != null)
            {
                var tagsString = tagsProp.GetValue<string>();
                var tags = tagsString.Split(';').Where(x => !string.IsNullOrWhiteSpace(x));

                foreach (var tag in tags)
                {
                    var exited = await context.FirstOrDefaultAsync(y => y.TagId == tag);
                    if (exited == null)
                    {
                        exited = new EntityTag()
                        {
                            TagId = tag.Trim().ToUpper(),
                        };
                    }
                    context.Add(exited);
                    yield return exited;
                }
            }
        }
    }
}