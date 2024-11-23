using System.Runtime.CompilerServices;

namespace IziHardGames.IziLibrary.ForCsproj
{
    public class Dicsover
    {
        public IAsyncEnumerable<FileInfo> FindCsProjAsync(IEnumerable<string> dirs, [EnumeratorCancellation] CancellationToken ct = default)
        {
            return FindCsProjAsync(dirs.Select(x => new DirectoryInfo(x)), ct);
        }
        public async IAsyncEnumerable<FileInfo> FindCsProjAsync(IEnumerable<DirectoryInfo> dirs, [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var dirInfo in dirs)
            {
                if (dirInfo.Exists)
                {
                    var files = dirInfo.GetFiles().Where(x => x.Extension == ".csproj");
                    foreach (var fi in files)
                    {
                        yield return fi;
                    }
                    var subs = FindCsProjAsync(dirInfo.GetDirectories(), ct);

                    await foreach (var sub in subs.WithCancellation(ct))
                    {
                        yield return sub;
                    }
                }
            }
        }
    }
}
