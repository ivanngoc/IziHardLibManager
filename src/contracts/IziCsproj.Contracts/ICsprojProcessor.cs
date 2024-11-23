using System.Threading.Tasks;

namespace IziHardGames.DotNetProjects
{
    public interface ICsprojProcessor
    {
        Task EnsureRequiredMetasAsync(ICsproj csproj);
        Task BeautifyAsync(ICsproj csproj);
    }
}
