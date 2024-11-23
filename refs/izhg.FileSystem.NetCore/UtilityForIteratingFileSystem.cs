using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IziHardGames.FileSystem.NetCore
{
    public static class UtilityForIteratingFileSystem
    {
        public static IEnumerable<FileInfo> GetAllFilesExcept(DirectoryInfo directoryInfo, FileAttributes filter)
        {
            foreach (var item in GetAllFiles(directoryInfo))
            {
                if (item.Attributes.HasFlag(filter)) continue;
                yield return item;
            }
        }
        public static IEnumerable<FileInfo> GetAllFilesWithExtension(DirectoryInfo directoryInfo, string extension, bool excludeJunctions = true)
        {
            foreach (var file in GetAllFiles(directoryInfo, excludeJunctions))
            {
                if (!file.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)) continue;
                yield return file;
            }
        }
        public static IEnumerable<FileInfo> GetAllFiles(DirectoryInfo directoryInfo, bool excludeJunctions = true)
        {
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                yield return file;
            }
            var subDris = directoryInfo.GetDirectories();
            foreach (var subdir in subDris)
            {
                if (excludeJunctions && IsJunction(subdir)) continue;
                var subFiles = GetAllFiles(subdir);
                foreach (var file in subFiles)
                {
                    yield return file;
                }
            }
        }

        public static IEnumerable<FileInfo> GetAllFiles(DirectoryInfo directoryInfo, Func<FileInfo, bool> filter, bool excludeJunctions = true)
        {
            var files = GetAllFiles(directoryInfo, excludeJunctions);

            foreach (var item in files)
            {
                if (filter(item)) yield return item;
            }
        }
      
        private static bool IsJunction(DirectoryInfo subdir)
        {
            return (subdir.Attributes != FileAttributes.Directory);
        }
    }
}
