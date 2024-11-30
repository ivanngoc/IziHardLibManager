using System.Threading.Tasks;

namespace IziHardGames.DotNetProjects
{
    public interface ICsprojProcessor
    {
        Task EnsureRequiredMetasAsync(ICsproj csproj);
        Task BeautifyAsync(ICsproj csproj);
        Task<int> FillRelationsAsParentsAsync();
        Task<int> FillRelationsAsChildsByIncludeFileExistingAsync();
        Task<int> FillRelationsAsChildsByCsprojFileNameAsync();
        Task<int> ReplaceChildIncludeAsync(string find, string replace);
        Task<int> FormatDependecies();
        Task<int> DistinctRelationsAsync();
        Task<int> FormatIncludePathToEnvVarBasedPathAsync();
        Task<int> ReplaceAbsIncludesWithRelativeAsync();
    }
}
