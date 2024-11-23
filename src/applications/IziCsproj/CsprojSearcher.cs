using System.IO;
using System.Collections.Generic;
using System.Linq;
using IziHardGames.FileSystem.NetCore;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.DotNetProjects
{
    public class CsprojSearcher : ICsproSearcher
    {
        public IEnumerable<FileInfo> FindMyCsprojs()
        {
            var excludeDirs = new List<string>()
            {
                "C:\\Users\\ngoc\\Documents\\GitHub",
                "C:\\Users\\ngoc\\Documents\\GCE",
                "C:\\Users\\ngoc\\Documents\\Packages\\zlib-1.2.12"
            };

            var dirs = new List<string>()
            {
                @"C:\Users\ngoc\Documents"
            };

            var excludeFilenameStartsWith = new List<string>()
            {
                "UnityEditor.TestRunner",
                "UnityEngine.TestRunner",
                "Unity.",
            };


            IEnumerable<FileInfo> csprojsFullPaths = Enumerable.Empty<FileInfo>();
            foreach (var dir in dirs)
            {
                var files = UtilityForIteratingFileSystem.GetAllFiles(new DirectoryInfo(dir), (x) =>
                {
                    return x.BeginQuery().HasExtension(".csproj").ExcludeSubdirs(excludeDirs).ExcludeFileNameStartWith(excludeFilenameStartsWith).End();
                });
                csprojsFullPaths = csprojsFullPaths.Concat(files);
            }
            return csprojsFullPaths;
        }
    }
}
