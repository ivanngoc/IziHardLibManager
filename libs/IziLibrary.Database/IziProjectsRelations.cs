using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziProjectsRelations
    {
        public static async Task TrackCsproj()
        {
            using ModulesDbContextV1 context = new ModulesDbContextV1();
            var myAsmdefs = await context.UnityAsmdefs.Include(x => x.Module).Where(x => !x.IsThirdParty && !x.IsNoUnityEngingeRef).ToArrayAsync();

            foreach (var asmdef in myAsmdefs)
            {
                var relation = context.Relations.Include(x => x.From).Include(x => x.To).FirstOrDefault(x => x.From!.Guid == asmdef.Module!.Guid && x.To!.Type == (uint)EModuleType.Csproj);
                if (relation != null)
                {
                    var iziCsproj = context.Csprojs.Include(x => x.Module).First(x => x.Module!.Guid == relation.To!.Guid);
                    /// возможно до этого не получалось найти связи потому что <see cref="InfoUnityMeta"/> устанавливался тот же GUID что у asmdef?
                    await IziEnsureCsproj.EnsureUnityDll(iziCsproj.PathFull, iziCsproj.Module!.Guid).ConfigureAwait(false); 
                    
                }
            }
        }
    }
}