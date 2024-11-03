using System.IO;
using System;
using IziHardGames.Libs.IziLibrary.Contracts;
using System.Threading.Tasks;

namespace IziHardGames.IziLibrary.Metas.Factories
{
    public abstract class MetaAbstract
    {
        public string? Directory => DirectoryInfo?.FullName;
        public string? FileName => FileInfo?.Name;
        public ProjectItem ProjectItem { get; private set; } = new ProjectItem();
        public FileInfo? FileInfo { get; private set; }
        public DirectoryInfo DirectoryInfo { get => FileInfo?.Directory ?? throw new NullReferenceException(); }
        public abstract string GetExtension();

        public MetaAbstract(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo;
            ProjectItem.FileName = fileInfo.FullName;
            ProjectItem.Directory = fileInfo.Directory!.FullName;
        }
    }
}
