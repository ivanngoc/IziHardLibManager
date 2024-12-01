using IziHardGames.Asmdefs.Contracts;
using IziHardGames.DotNetProjects;
using IziHardGames.FileSystem.NetCore;
using IziLibrary.Database.DataBase.EfCore;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Asmdefs.Models;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Asmdefs
{
    public class AsmdefSearcher(IziProjectsDbContext context) : IAsmdefSearcher
    {
        public async Task<int> DiscoverAndSaveAsync()
        {
            var files = Discover();
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var dic = new Dictionary<AsmdefId, EntityAsmdef?>();
            var asmdefs = await context.Asmdefs.ToArrayAsync();
            var asmdefsAtDevice = await context.AsmdefsAtDevice.ToArrayAsync();

            foreach (var fi in files)
            {
                var asmdef = new Asmdef(fi);
                EntityAsmdef? existed = null;
                var guid = await asmdef.GetGuidAsync();
                if (guid.HasValue)
                {
                    existed = asmdefs.FirstOrDefault(x => x.EntityAsmdefId == guid.Value);
                    dic.Add(guid.Value, existed);
                    if (existed == null)
                    {
                        existed = new EntityAsmdef()
                        {
                            EntityAsmdefId = guid.Value,
                        };
                        context.Asmdefs.Add(existed);
                    }
                    var existedAtDevice = asmdefsAtDevice.FirstOrDefault(x => x.DeviceId == idDevice && x.AsmdefId == guid.Value);
                    if (existedAtDevice == null)
                    {
                        existedAtDevice = new EntityAsmdefAtDevice()
                        {
                            DeviceId = idDevice,
                            AsmdefId = existed.EntityAsmdefId,
                            //Asmdef = default,
                        };
                        context.AsmdefsAtDevice.Add(existedAtDevice);
                    }
                    existedAtDevice.PathAbs = fi.FullName;
                }
            }

            //var first = await context.AsmdefsAtDevice.AsTracking().FirstOrDefaultAsync();

            //foreach (var item in asmdefsAtDevice!)
            //{
            //    if (context.Entry(item).State == EntityState.Modified)
            //    {
            //        Console.WriteLine();
            //    }
            //}

            return await context.SaveChangesAsync();
        }

        public IEnumerable<FileInfo> Discover()
        {
            var excludeDirs = Enumerable.Empty<string>();
            var dirs = Enumerable.Empty<string>();
            var excludeFilenameStartsWith = Enumerable.Empty<string>();

            if (IziEnvironmentsHelper.IsMyPcVnn())
            {
                excludeDirs = new List<string>()
                {
                    "C:\\Users\\ngoc\\Documents\\GitHub",
                    "C:\\Users\\ngoc\\Documents\\GCE",
                    "C:\\Users\\ngoc\\Documents\\Packages\\zlib-1.2.12"
                };

                dirs = new List<string>()
                {
                    @"C:\Users\ngoc\Documents"
                };

                excludeFilenameStartsWith = new List<string>()
                {
                };
            };

            if (IziEnvironmentsHelper.IsMyPcVnn())
            {
                dirs = new List<string>()
                {
                    @"C:\Users\ivan\Documents"
                };
                excludeDirs = new List<string>()
                {
                    @"C:\Users\ivan\Documents\GCE"
                };
            }

            IEnumerable<FileInfo> csprojsFullPaths = Enumerable.Empty<FileInfo>();
            foreach (var dir in dirs)
            {
                var files = UtilityForIteratingFileSystem.GetAllFiles(new DirectoryInfo(dir), (x) =>
                {
                    return x.BeginQuery()
                            .HasExtension(".asmdef")
                            .ExcludeSubdirs(excludeDirs)
                            .ExcludeFileNameStartWith(excludeFilenameStartsWith)
                            .End();
                });
                csprojsFullPaths = csprojsFullPaths.Concat(files);
            }
            return csprojsFullPaths;
        }
    }
}
