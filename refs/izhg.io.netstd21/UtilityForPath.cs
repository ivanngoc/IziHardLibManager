using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static IziHardGames.Environments.IziEnvironments;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class UtilityForPath
    {
        /// <summary>
        /// C:\buildstemp\$(ProjectName)\$(Configuration)\bin, где $(Configuration) - переменная среды <br/>
        /// $(IZHG_LIB_CONTROL_DIR_FOR_REFS)\izhg.FileSystem.NetCore\izhg.FileSystem.NetCore.csproj
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetActualAbsolutePath(string path, [NotNull] string? basePath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));
            // replace %ENV_NAME% but not $(ENV_NAME)
            var result = Environment.ExpandEnvironmentVariables(path);
            result = path.Replace(@$"$({IZHG_LIB_CONTROL_DIR_FOR_REFS})", Environment.GetEnvironmentVariable(IZHG_LIB_CONTROL_DIR_FOR_REFS));
            result = path.Replace(@$"$({IZHG_MODULES})", Environment.GetEnvironmentVariable(IZHG_LIB_CONTROL_DIR_FOR_REFS));
            if (IsRelative(result))
            {
                return RelativeToAbsolute(basePath, result);
            }
            return path;
        }
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

    }
}
