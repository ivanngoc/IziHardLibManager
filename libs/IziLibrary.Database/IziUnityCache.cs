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
            using ModulesDbContextV1 context = new ModulesDbContextV1();

            foreach (var dir in dirs)
            {
                DirectoryInfo info = new DirectoryInfo(dir);
                var files = info.SelectAllFilesBeneath().Where(x => OldInfoAsmdef.IsValidExtension(x));

                foreach (var file in files)
                {
                    Guid guid = await OldInfoAsmdef.FindGuidFromMetaAsync(file).ConfigureAwait(false);
                    if (context.UnityAsmdefs.Include(x => x.Module).Any(x => x.Module!.Guid != guid))
                    {
                        OldInfoAsmdef infoAsmdef = new OldInfoAsmdef(file);
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
            using ModulesDbContextV1 context = new ModulesDbContextV1();
            return context.IsThirdPartyAsmdef(guid);
        }
    }
}
