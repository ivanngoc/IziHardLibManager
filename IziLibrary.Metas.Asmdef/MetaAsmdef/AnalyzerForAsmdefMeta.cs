global using @AnalyzerMeta = IziHardGames.IziLibrary.Metas.Factories.Contracts.IAnalyzer<IziHardGames.IziLibrary.Metas.ForAsmdef.MetaForAsmdefMeta, IziHardGames.IziLibrary.Metas.ForAsmdef.MetaAnalyzForAsmdefMeta>;

using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;
using static IziHardGames.IziLibrary.Contracts.ConstantsForIziLibrary;
namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class AnalyzerForAsmdefMeta : @AnalyzerMeta
    {
        public async ValueTask<MetaAnalyzForAsmdefMeta> ExecuteAsync(MetaForAsmdefMeta from)
        {
            var fi = from.FileInfo ?? throw new NullReferenceException();
            var guid = await GetGuid(fi).ConfigureAwait(false);
            if (guid is null) throw new FormatException($"No guid Founded: {await File.ReadAllTextAsync(fi.FullName)}");

            var content = await File.ReadAllTextAsync(fi.FullName);

            MetaAnalyzForAsmdefMeta metaAnalyz = new MetaAnalyzForAsmdefMeta()
            {
                Content = content,
                Guid = guid,
            };
            return metaAnalyz;
        }

        public static async ValueTask<Guid?> GetGuid(FileInfo fi)
        {
            var lines = await File.ReadAllLinesAsync(fi.FullName).ConfigureAwait(false);

            foreach (var item in lines)
            {
                var line = item.Trim();
                if (line.StartsWith("guid:", StringComparison.InvariantCultureIgnoreCase))
                {
                    var split = line.Split(':');
                    if (Guid.TryParse(split[1], out var guid))
                    {
                        return guid;
                    }
                }
            }
            return null;
        }
        public static async Task EnsureGuidAsync(FileInfo fi)
        {
            var result = await GetGuid(fi);
            if (result is null) throw new FormatException($"No guid Founded: {await File.ReadAllTextAsync(fi.FullName)}");
        }
    }
}
