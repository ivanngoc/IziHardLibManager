using System.IO;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;

namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class DetectorForAsmdef : IFileDetector<MetaForAsmdef?>
    {
        public MetaForAsmdef? Detect(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                if (fileInfo.Extension.Equals(MetaForAsmdef.EXTENSION_ASMDEF))
                {
                    return new MetaForAsmdef(fileInfo);
                }
            }
            return null;
        }
    }
}
