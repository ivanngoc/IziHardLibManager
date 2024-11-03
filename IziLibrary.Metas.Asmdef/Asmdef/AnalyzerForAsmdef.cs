global using @Analyzer = IziHardGames.IziLibrary.Metas.Factories.Contracts.IAnalyzer<IziHardGames.IziLibrary.Metas.ForAsmdef.MetaForAsmdef, IziHardGames.IziLibrary.Metas.ForAsmdef.MetaAnalyzForAsmdef>;
using IziHardGames.Contracts;
using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;
using static IziHardGames.IziLibrary.Contracts.ConstantsForIziLibrary;
using IziHardGames.IziLibrary.Contracts;
using System.Threading.Tasks;
using System.IO;

namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class AnalyzerForAsmdef : IAnalyzer<MetaForAsmdef, MetaAnalyzForAsmdef>
    {
        private readonly @AnalyzerMeta analyzerMeta;

        public AnalyzerForAsmdef(@AnalyzerMeta analyzerMeta)
        {
            this.analyzerMeta = analyzerMeta;
        }

        public async ValueTask<MetaAnalyzForAsmdef> ExecuteAsync(MetaForAsmdef from)
        {
            var fi = from.FileInfo;
            await EnsureGuidAsync(fi).ConfigureAwait(false);
            var content = await File.ReadAllTextAsync(fi.FullName);
            var json = JsonObject.Parse(content) ?? throw new NullReferenceException();

            MetaAnalyzForAsmdef metaAnalyz = new MetaAnalyzForAsmdef()
            {
                Content = content,
                Guid = Guid.Parse(json[Asmdef.JSON_FIELD_NAME_UNITY_META_GUID]!.GetValue<string>()),
            };
            return metaAnalyz;
        }

        private async Task EnsureGuidAsync(FileInfo? fi)
        {
            var content = await File.ReadAllTextAsync(fi.FullName);
            var json = JsonObject.Parse(content);

            if (json[Asmdef.JSON_FIELD_NAME_UNITY_META_GUID] is null)
            {
                var meta = MetaForAsmdefMeta.FindForAsmdef(fi);
                var analyz = await analyzerMeta.ExecuteAsync(meta);
                ArgumentNullException.ThrowIfNull(analyz);
                json[Asmdef.JSON_FIELD_NAME_UNITY_META_GUID] = IziLibraryFormat.ToString(analyz.Guid!.Value);
                await File.WriteAllTextAsync(fi.FullName, json.ToString());
            }
            else
            {
                var guidString = json[Asmdef.JSON_FIELD_NAME_UNITY_META_GUID].GetValue<string>();

                if (!Guid.TryParse(guidString, out Guid guid))
                {
                    throw new FormatException($"Invalid guid!: {content}");
                }
            }
        }
    }
}
