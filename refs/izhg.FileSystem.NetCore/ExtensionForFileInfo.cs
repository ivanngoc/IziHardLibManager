namespace IziHardGames.FileSystem.NetCore
{
    public static class ExtensionForFileInfo
    {
        public static IEnumerable<FileInfo> ExcludeDirs(this IEnumerable<FileInfo> files, IEnumerable<string> excludeDirs)
        {
            foreach (var file in files)
            {
                foreach (var dirExclude in excludeDirs)
                {
                    string fullPathA = file.FullName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    string fullPathB = Path.GetFullPath(dirExclude).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    if (fullPathA.StartsWith(fullPathB, StringComparison.OrdinalIgnoreCase))
                    {
                        goto SKIP;
                    }
                }
                yield return file;
                SKIP: continue;
            }
        }
    }
}
