using System.IO;

namespace IziHardGames.IziLibrary.Metas.Factories.Contracts
{
    public interface IFileDetector<out T>
    {
        T Detect(FileInfo fileInfo);
    }
}
