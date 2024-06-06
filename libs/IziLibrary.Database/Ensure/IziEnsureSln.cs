using System;
using System.IO;
using System.Threading.Tasks;

namespace IziHardGames.Projects
{
    public static class IziEnsureSln
    {
        public static async Task FormatProjectSingle(FileInfo file)
        {
            InfoSln infoSln = new InfoSln(file);
            await infoSln.ExecuteAsync().ConfigureAwait(false);
        }
    }
}