using System;
using System.IO;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class UtilityForFileInfo
    {
        public static FileInfo Backup(FileInfo target)
        {
            return target.CopyTo(target.FullName + ".backup", true);
        }

        public static string Delta(DirectoryInfo root, FileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        public static bool IsSameFile(FileInfo fileInfo, FileInfo csproj)
        {
            return string.Equals(fileInfo.FullName, csproj.FullName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
