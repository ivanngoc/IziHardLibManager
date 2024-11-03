using System.Threading.Tasks;

namespace IziHardGames.IziLibrary.Metas.Factories.Contracts
{
    public interface IAnalyzer<TFrom, TTo>
        where TFrom : MetaAbstract
        where TTo : MetaAnalyz
    {
        ValueTask<TTo> ExecuteAsync(TFrom from);
    }
}
