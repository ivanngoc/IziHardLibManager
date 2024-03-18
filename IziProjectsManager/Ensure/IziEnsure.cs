using System;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase;

namespace IziHardGames.Projects
{
    public static class IziEnsure
    {
        internal static async Task IziMetaDllJson(DirectoryInfo directory)
        {
            string fullPath = Path.Combine(directory.FullName, InfoDll.FILE_NAME);
            InfoDll? infoDll = default;

            if (!File.Exists(fullPath))
            {
                infoDll = await InfoDll.CreateDefaultAsync(fullPath).ConfigureAwait(false);
            }
            else
            {
                infoDll = await IziProjectsActualization.UpdateInfoDllAsync(directory, fullPath).ConfigureAwait(false);
                if (infoDll.IsChanged)
                {
                    using ModulesDbContext context = new ModulesDbContext();
                    await context.AddOrUpdateAsync(infoDll).ConfigureAwait(false);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
        internal static async ValueTask<InfoIziProjectsMeta> IziMetaAsync(DirectoryInfo directory)
        {
            string fullPath = Path.Combine(directory.FullName, InfoIziProjectsMeta.META_NAME);
            InfoIziProjectsMeta? iziProjectsMeta = null;

            if (!File.Exists(fullPath))
            {
                iziProjectsMeta = await InfoIziProjectsMeta.CreateDefaultFileAsync(directory);
            }
            else
            {
                FileInfo fileInfo = new FileInfo(fullPath);
                iziProjectsMeta = new InfoIziProjectsMeta(fileInfo);
            }
            return iziProjectsMeta ?? throw new NullReferenceException($"Can't found/create {fullPath}");
        }
    }
}