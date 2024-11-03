global using @Transformer = IziHardGames.Contracts.ITransformer<(IziHardGames.IziLibrary.Metas.ForAsmdef.MetaForAsmdef, IziHardGames.IziLibrary.Metas.ForAsmdef.MetaAnalyzForAsmdef), IziHardGames.Libs.IziLibrary.Contracts.ModelAsmdef>;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Contracts;
using IziHardGames.IziLibrary.Metas.ForAsmdef;
using IziHardGames.Libs.IziLibrary.Contracts;

namespace IziHardGames.IziLibrary.Metas.Linked.Transformers
{
    public class TransformMetaForAsmdefToModelAsmdef : @Transformer
    {
        public ModelAsmdef Transform((MetaForAsmdef, MetaAnalyzForAsmdef) input)
        {
            var from = input.Item1;
            return new ModelAsmdef()
            {
                FileName = from.FileName ?? "Error",
                Directory = from.Directory ?? throw new NullReferenceException(),
                Guid = input.Item2.Guid ?? throw new NullReferenceException(),
            };
        }
    }
}
