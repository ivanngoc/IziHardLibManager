using System.IO;
using IziHardGames.IziLibrary.Metas.Factories;

namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class MetaForAsmdefMeta : MetaAbstract
    {
        public const string EXTENSION_ASMDEF_META = ".meta";

        public MetaForAsmdefMeta(FileInfo fileInfo) : base(fileInfo)
        {

        }

        public static MetaForAsmdefMeta FindForAsmdef(FileInfo fi)
        {
            string fullname = fi.FullName + EXTENSION_ASMDEF_META;
            if (!File.Exists(fullname))
            {
                throw new FileNotFoundException(fullname);
            }
            return new MetaForAsmdefMeta(new FileInfo(fullname));
        }

        public override string GetExtension()
        {
            return EXTENSION_ASMDEF_META;
        }
    }
}
