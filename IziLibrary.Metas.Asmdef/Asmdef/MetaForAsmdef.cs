using System.IO;
using IziHardGames.IziLibrary.Metas.Factories;
using IziHardGames.Libs.IziLibrary.Contracts;

namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class MetaForAsmdef : MetaAbstract
    {
        public const string EXTENSION_ASMDEF = ".asmdef";

        public MetaForAsmdef(FileInfo fileInfo) : base(fileInfo)
        {
          
        }

        public override string GetExtension()
        {
            return EXTENSION_ASMDEF;
        }
    }
}
