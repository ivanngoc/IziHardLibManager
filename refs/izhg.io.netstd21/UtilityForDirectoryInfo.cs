using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IziHardGames.FileSystem.NetStd21
{
	public static class UtilityForDirectoryInfo
    {
        public static bool TryGetDelta(DirectoryInfo root, FileInfo fileInfo, out string delta)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Can navigate backward
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string GetRelativePath(DirectoryInfo root, FileInfo fileInfo)
        {
            string basePath = Path.GetRelativePath(root.FullName, fileInfo.Directory.FullName);
            char last = basePath.Last();
            if (last == '\\' || last == '/')
            {
                return basePath + fileInfo.Name;
            }
            else
            {
                if (basePath.Contains('\\'))
                {
                    return basePath + Path.DirectorySeparatorChar + fileInfo.Name;
                }
                else
                {
                    return basePath + Path.AltDirectorySeparatorChar + fileInfo.Name;
                }
            }
        }
        public static bool TryGetDelta(DirectoryInfo from, DirectoryInfo to, out string delta)
        {
            if (from.IsNested(to))
            {
                delta = UtilityForPath.GetDeltaFromAbsToAbs(from.FullName, to.FullName);
                return true;
            }
            delta = string.Empty;
            return false;
        }
        public static void SearchRecursive(DirectoryInfo dir, List<FileInfo> result, Func<FileInfo, bool> predictate)
        {
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                if (predictate(file))
                {
                    result.Add(file);
                }
            }
            var subdirs = dir.GetDirectories();

            foreach (var subdir in subdirs)
            {
                SearchRecursive(subdir, result, predictate);
            }
        }
#if DEBUG
        private static void Test()
        {
            var d1 = new DirectoryInfo("C:\\Users\\ngoc\\Documents\\");
            var d2 = new DirectoryInfo("C:\\Users\\ngoc\\Documents");
            var v1 = d1.FullName; // "C:\\Users\\ngoc\\Documents\\"
            var v2 = d2.FullName; // "C:\\Users\\ngoc\\Documents"
        }
#endif
    }
}
