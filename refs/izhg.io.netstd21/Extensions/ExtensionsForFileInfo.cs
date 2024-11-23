using System.IO;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class ExtensionsForFileInfo
    {
        public static string FileNameWithoutExtension(this FileInfo info)
        {
            string name = info.Name;
            return name.Substring(0, name.Length - info.Extension.Length);
        }
    }
}
