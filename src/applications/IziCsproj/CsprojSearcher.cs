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
            var excludeDirs = Enumerable.Empty<string>();
            var dirs = Enumerable.Empty<string>();
            var excludeFilenameStartsWith = Enumerable.Empty<string>();

            if (IziEnvironmentsHelper.IsMyPcVnn())
            {
                excludeDirs = new List<string>()
                {
                    "C:\\Users\\ngoc\\Documents\\GitHub",
                    "C:\\Users\\ngoc\\Documents\\GCE",
                    "C:\\Users\\ngoc\\Documents\\Packages\\zlib-1.2.12"
                };

                dirs = new List<string>()
                {
                    @"C:\Users\ngoc\Documents"
                };

                excludeFilenameStartsWith = new List<string>()
                {
                    "UnityEditor.TestRunner",
                    "UnityEngine.TestRunner",
                    "Unity.",
                };
            };

            if (IziEnvironmentsHelper.IsMyPcVnn())
            {
                dirs = new List<string>()
                {
                    @"C:\Users\ivan\Documents"
                };
            }

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
