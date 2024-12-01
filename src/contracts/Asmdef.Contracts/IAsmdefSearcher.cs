using System.Threading.Tasks;

namespace IziHardGames.Asmdefs.Contracts
{
    public interface IAsmdefSearcher
    {
        Task<int> DiscoverAndSaveAsync();
    }
}
