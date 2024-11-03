using System;
using System.Collections.Generic;
using System.Threading;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;

namespace IziHardGames.IziLibrary.Metas.ForAsmdef
{
    public class SourceFiltered : ISource<MetaForAsmdef>
    {
        public IAsyncEnumerable<MetaForAsmdef> GetSource(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
