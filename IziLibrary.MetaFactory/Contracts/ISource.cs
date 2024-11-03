using System.Collections.Generic;
using System.Threading;

namespace IziHardGames.IziLibrary.Metas.Factories.Contracts
{
    public interface ISource<T>
    {
        IAsyncEnumerable<T> GetSource(CancellationToken ct = default);
    }
}
