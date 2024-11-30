using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.IziLibrary.Metas.Factories;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;

namespace IziLibrary.Commands.FileSystem
{
    public class MetaProviderFromFileSystem : IMetaProvider<MetaAbstract>
    {
        private IEnumerable<IFileDetector<MetaAbstract>> detectors;
        private ISource<FileInfo> source;

        public MetaProviderFromFileSystem(IEnumerable<IFileDetector<MetaAbstract>> detectors, ISource<FileInfo> source)
        {
            this.detectors = detectors;
            this.source = source;
        }

        public async IAsyncEnumerable<MetaAbstract> Provide(CancellationToken ct = default)
        {
            await foreach (var file in source.GetSource().WithCancellation(ct).ConfigureAwait(false))
            {
                foreach (var detector in detectors)
                {
                    var result = detector.Detect(file);
                    if (result != null) yield return result;
                }
            }
        }
    }
}
