using System;
using System.IO;

namespace IziHardGames.DotNetProjects
{
    public class Csproj : ICsproj
    {
        public FileInfo FileInfo { get; set; }
        public string FilePathAbsolute { get => FileInfo.FullName; set => throw new NotSupportedException(); }

        public Csproj(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo;
        }
    }
}
