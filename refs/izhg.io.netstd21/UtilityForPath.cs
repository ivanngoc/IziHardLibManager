using System;
using System.IO;
using System.Linq;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class UtilityForPath
    {
       
        public static bool IsValidPath(string path, out DirectoryInfo info)
        {
            info = new DirectoryInfo(path);
            return info.Exists;
        }
        public static bool IsValidPath(string path) => throw new System.NotImplementedException();
        public static bool IsRelative(string path)
        {
            return !Path.IsPathRooted(path);
            if (path.Contains(':')) return false;
            // относительный переход может быть как вперед так и назад
            //if (basePath.StartsWith("..\\")) return true;
            return true;
        }

        /// <summary>
        /// dirRootAbs: C:\Users\ngoc\Documents\<br/>
        /// dirNestedAbs: C:\Users\ngoc\Documents\NetedFolder\<br/><br/>
        /// For relative Must be from same root<br/>
        /// dirRootRel: ..\ngoc\Documents\<br/>
        /// dirNestRel: ..\ngoc\Documents\NetedFolder<br/>
        /// </summary>
        /// <param name="dirRootAbs"></param>
        /// <param name="dirNestedAbs"></param>
        /// <returns>..\NetedFolder\</returns>
        /// <exception cref="NotImplementedException"></exception>
		internal static string GetDeltaFromAbsToAbs(string dirRootAbs, string dirNestedAbs)
        {
            var substring = dirNestedAbs.Substring(dirRootAbs.Length);
            if (substring.Last() == '\\')
            {
                if (substring.StartsWith('\\')) return @".." + substring;
                return @"..\" + substring;
            }
            else
            {
                if (substring.StartsWith('\\')) return @".." + substring + '\\';
                return @"..\" + substring + '\\';
            }
        }

        public static string Combine(DirectoryInfo anchor, string relativePath, char separator)
        {
            int backwardsCount = 0;
            int offset = default;
            var scan = relativePath.AsSpan();
            for (int i = 0; i < relativePath.Length; i += 3)
            {
                if (scan.Slice(i).StartsWith("..")) backwardsCount++;
                else
                {
                    offset = i;
                    break;
                }
            }
            if (relativePath[offset] == Path.DirectorySeparatorChar || relativePath[offset] == Path.AltDirectorySeparatorChar) offset++;

            DirectoryInfo current = anchor;
            for (int i = 0; i < backwardsCount; i++)
            {
                current = current.Parent;
            }
            var lastChar = current.FullName.Last();
            if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
            {
                return current.FullName + relativePath.Substring(offset);
            }
            return current.FullName + separator + relativePath.Substring(offset);
        }
        public static string RelativeToAbsolute(string basePath, string relativePath)
        {
            string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
            return fullPath;
        }

        [WindowsDirSeparator]
        public static bool SamePath(FileInfo fileInfo, string absPathToItem)
        {
            var delta = absPathToItem.Length - fileInfo.FullName.Length;
            if (delta > 0) return false;
            return string.Compare(fileInfo.FullName, absPathToItem, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string AbsToRelative(DirectoryInfo directory, string pathAbs)
        {
            return Path.GetRelativePath(directory.FullName, pathAbs);
        }

        public static bool IsSameDir(DirectoryInfo a, DirectoryInfo b)
        {
            return a.FullName.TrimEnd('\\').TrimEnd('/') == b.FullName.TrimEnd('\\').TrimEnd('/');
        }

        public static char GetSeparator(string path)
        {
            if (path.Contains(Path.DirectorySeparatorChar)) return Path.DirectorySeparatorChar;
            if (path.Contains(Path.AltDirectorySeparatorChar)) return Path.AltDirectorySeparatorChar;
            throw new System.NotImplementedException();
        }

        public static bool ComparePaths(string a, string b)
        {
            return string.Equals(a.TrimEnd('\\'), b.TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetFileNameWithoutExtension(string pathAbs)
        {
            return new FileInfo(pathAbs).FileNameWithoutExtension();
        }

        public static bool TryAbsToRelative(FileInfo from, FileInfo to, out string? pathRel)
        {
            pathRel = null;
            // Get absolute paths
            string absParent = from.Directory.Root.FullName;
            string absChild = to.Directory.Root.FullName;

            // Ensure both files exist
            if (!from.Exists || !to.Exists)
                return false;

            // Check if both paths have a common root
            if (!absChild.StartsWith(Path.GetDirectoryName(absParent) ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                return false;

            // Get relative path
            Uri parentUri = new Uri(from.Directory.FullName);
            Uri childUri = new Uri(to.FullName);
            pathRel = Uri.UnescapeDataString(parentUri.MakeRelativeUri(childUri).ToString());

            // Convert to correct directory separator if needed
            pathRel = pathRel.Replace('/', Path.DirectorySeparatorChar);

            return true;
        }
    }
}
