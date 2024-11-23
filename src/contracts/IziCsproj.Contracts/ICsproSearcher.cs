using System.Collections.Generic;
using System.IO;

namespace IziHardGames.DotNetProjects
{
    public interface ICsproSearcher
    {
        IEnumerable<FileInfo> FindMyCsprojs();
    }
}
