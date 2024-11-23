using System;
using System.Collections.Generic;
using System.IO;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class ExtensionsForFileInfo
    {
        public static (bool, FileInfo) BeginQuery(this FileInfo pair)
        {
            return (true, pair);
        }
        public static bool End(this (bool, FileInfo) pair)
        {
            return pair.Item1;
        }
        public static (bool, FileInfo) HasExtension(this (bool, FileInfo) pair, ReadOnlySpan<char> extension)
        {
            if (pair.Item1)
            {
                return (pair.Item2.Extension.AsSpan().SequenceEqual(extension), pair.Item2);
            }
            return (false, pair.Item2);
        }
        public static (bool, FileInfo) ExcludeSubdirs(this (bool, FileInfo) pair, IEnumerable<string> dirs)
        {
            var file = pair.Item2;
            if (pair.Item1)
            {
                foreach (var dirExclude in dirs)
                {
                    string fullPathA = file.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    string fullPathB = Path.GetFullPath(dirExclude).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    if (fullPathA.StartsWith(fullPathB, StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, file);
                    }
                }
            }
            return (pair.Item1, file);
        }
        public static (bool, FileInfo) ExcludeFileNameStartWith(this (bool, FileInfo) pair, IEnumerable<string> filenames)
        {
            var file = pair.Item2;
            if (pair.Item1)
            {
                foreach (var start in filenames)
                {
                    if (file.Name.StartsWith(start))
                    {
                        return (false, file);
                    }
                }
            }
            return (pair.Item1, file);
        }
        public static string FileNameWithoutExtension(this FileInfo info)
        {
            string name = info.Name;
            return name.Substring(0, name.Length - info.Extension.Length);
        }
    }
}
