using System;
using System.Threading.Tasks;

namespace Metas.Contracts
{
    public interface IMetaSearcher
    {
        Task<int> DiscoverLinkedToAsmdefsAndSaveAsync();
    }
}
