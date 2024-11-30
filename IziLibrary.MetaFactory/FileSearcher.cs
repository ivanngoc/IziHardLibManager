using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;

namespace IziHardGames.IziLibrary.Metas.Factories
{
    public class FileSearcher : IFileSearcher
    {
        private IFileDetector<MetaAbstract> fileDetector;
        public FileSearcher(IFileDetector<MetaAbstract> fileDetector)
        {
            this.fileDetector = fileDetector;
        }

        public async IAsyncEnumerable<MetaAbstract> ScanDirectoriesAsync(string[] dirs, [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                var result = ScanDirectoryAsync(di);

                await foreach (var item in result.WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }
        public async IAsyncEnumerable<MetaAbstract> ScanDirectoryAsync(DirectoryInfo directory, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {

            }
            var dirs = directory.GetDirectories();
            foreach (var dir in dirs)
            {
                await foreach (var item in ScanDirectoryAsync(dir, ct).WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }
    }
}
