using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;

namespace IziHardGames.IziLibrary.Metas.Factories
{
    public class SourceForFiles : ISource<FileInfo>
    {
        private FileSystemScanConfig config;

        public SourceForFiles(FileSystemScanConfig config)
        {
            this.config = config;
        }

        public async IAsyncEnumerable<FileInfo> GetSource(CancellationToken ct = default)
        {
            foreach (var item in config.dirs)
            {
                DirectoryInfo info = new DirectoryInfo(item);
                if (info.Exists)
                {
                    await foreach (var file in ItterateDirectory(info).WithCancellation(ct).ConfigureAwait(false))
                    {
                        yield return file;
                    }
                }
            }
        }

        public async IAsyncEnumerable<FileInfo> ItterateDirectory(DirectoryInfo directory, CancellationToken ct = default)
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                yield return file;
            }
            var dirs = directory.GetDirectories();

            foreach (var dir in dirs)
            {
                await foreach (var item in ItterateDirectory(dir, ct).WithCancellation(ct).ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }
    }
}
