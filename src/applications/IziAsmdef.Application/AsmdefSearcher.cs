﻿using IziHardGames.Asmdefs.Contracts;
using IziHardGames.DotNetProjects;
using IziHardGames.FileSystem.NetCore;
using IziLibrary.Database.DataBase.EfCore;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Asmdefs.Models;
using Microsoft.EntityFrameworkCore;
using YamlDotNet.Core.Tokens;

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
                EntityAsmdef? toProcess = null;
                var guid = await asmdef.GetGuidAsync();
                if (guid.HasValue)
                {
                    toProcess = asmdefs.FirstOrDefault(x => x.EntityAsmdefId == guid.Value);
                    try
                    {
                        dic.Add(guid.Value, toProcess);
                    }
                    catch (Exception ex)
                    {
                        var existed = dic[guid.Value];
                        throw new Exception($"Duplicate Guid. existed path: {existed.AsmdefsAtDevice.First().PathAbs}. Processed: {fi.FullName}", ex);
                    }
                    if (toProcess == null)
                    {
                        var tags = await asmdef.GetOrCreateTags(context.Tags).ToListAsync();
                        toProcess = new EntityAsmdef()
                        {
                            EntityAsmdefId = guid.Value,
                            Name = fi.FileNameWithoutExtension(),
                            Tags = tags,
                        };
                        context.Asmdefs.Add(toProcess);
                    }
                    var existedAtDevice = asmdefsAtDevice.FirstOrDefault(x => x.DeviceId == idDevice && x.AsmdefId == guid.Value);
                    if (existedAtDevice == null)
                    {
                        existedAtDevice = new EntityAsmdefAtDevice()
                        {
                            DeviceId = idDevice,
                            AsmdefId = toProcess.EntityAsmdefId,
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
                    @"C:\Users\ngoc\Documents",
                    @"C:\.izhg-lib"
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
                    @"C:\Users\ivan\Documents\GCE",
                    @"C:\Users\ivan\Documents\.unity\GameProject5\Library\"
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
