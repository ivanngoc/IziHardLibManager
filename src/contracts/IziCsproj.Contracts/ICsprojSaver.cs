using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IziHardGames.DotNetProjects
{
    public interface ICsprojSaver
    {
        Task<int> SaveToDbAsync(IEnumerable<FileInfo> csprojsFullPaths);
    }
}
