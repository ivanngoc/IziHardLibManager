using System.IO;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase;
using IziHardGames.FileSystem.NetStd21;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{
    public static class IziUnityCache
    {
        public static async Task InitilizeAsync()
        {
            var dirs = Config.DirsUnityCaches;
            using ModulesDbContext context = new ModulesDbContext();

            foreach (var dir in dirs)
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                var files = info.SelectAllFilesBeneath().Where(x => InfoAsmdef.IsValidExtension(x));

                foreach (var file in files)
                {
                    Guid guid = await InfoAsmdef.FindGuidFromMetaAsync(file).ConfigureAwait(false);
                    if (context.UnityAsmdefs.Include(x => x.Module).Any(x => x.Module!.Guid != guid))
                    {
                        InfoAsmdef infoAsmdef = new InfoAsmdef(file);
                        await infoAsmdef.ExecuteAsync().ConfigureAwait(false);
                        if (!infoAsmdef.IsGuidFounded) continue;
                        infoAsmdef.IsThirdParty = true;
                        await context.AddAsync(infoAsmdef);
                    }
                }
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        public static bool CheckMissing(Guid guid)
        {
            using ModulesDbContext context = new ModulesDbContext();
            return context.IsThirdPartyAsmdef(guid);
        }
    }
}
