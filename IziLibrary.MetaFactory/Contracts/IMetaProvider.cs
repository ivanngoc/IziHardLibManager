using System.Collections.Generic;
using System.Threading;

namespace IziHardGames.IziLibrary.Metas.Factories.Contracts
{
    public interface IMetaProvider<out T> where T : MetaAbstract
    {
        IAsyncEnumerable<T> Provide(CancellationToken ct = default);
    }
}
