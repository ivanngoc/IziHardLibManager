using System.IO;

namespace IziHardGames.DotNetProjects
{
    public interface ICsproj
    {
        public string FilePathAbsolute { get; set; }
        public FileInfo FileInfo{ get; set; }
    }
}
