using IziHardGames.Asmdefs;
using IziHardGames.DotNetProjects;
using IziLibrary.Database.DataBase.EfCore;
using Metas.Contracts;
using Microsoft.EntityFrameworkCore;
using Meta = IziHardGames.Metas.Models.Meta;

namespace IziMetas.Application
{
    public class MetaSearcher(IziProjectsDbContext context) : IMetaSearcher
    {
        public async Task<int> DiscoverLinkedToAsmdefsAndSaveAsync()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var q = context.AsmdefsAtDevice.Where(x => x.DeviceId == idDevice);
            var asmdefsAtDevice = await q.ToArrayAsync();

            foreach (var asmdefAtDevice in asmdefsAtDevice)
            {
                var pathAsmdef = asmdefAtDevice.PathAbs;
                var fiAsmdef = new FileInfo(pathAsmdef);
                var meta = Meta.FromAsmdef(fiAsmdef);
                EntityMeta? eMeta = null;
                if (meta != null)
                {
                    var guid = (MetaId?)(await meta.GetGuidAsmdefAsync());
                    ArgumentNullException.ThrowIfNull(guid);
                    if (guid.HasValue)
                    {
                        eMeta = await context.Metas.Where(x => x.MetaId == guid.Value).Include(x => x.Asmdef).FirstOrDefaultAsync();
                    }
                    if (eMeta == null)
                    {
                        eMeta = await context.Metas.Where(x => x.EntityMetaAtDevices.Any(y => y.PathAbs == meta.PathAbs)).Include(x => x.Asmdef).FirstOrDefaultAsync();
                    }
                    if (eMeta == null)
                    {
                        var eAsmdef = await context.Asmdefs.FirstOrDefaultAsync(x => x.EntityAsmdefId == (AsmdefId)guid.Value.Guid);
                        eMeta = new EntityMeta()
                        {
                            MetaId = (MetaId)guid,
                            Asmdef = eAsmdef,
                        };
                        context.Metas.Add(eMeta);
                    }

                    EntityMetaAtDevice? eMetaAtDevice = null;
                    eMetaAtDevice = await context.MetasAtDevice.Where(x => x.DeviceId == idDevice && (x.MetaId == eMeta.MetaId || x.PathAbs == meta.PathAbs)).FirstOrDefaultAsync();

                    if (eMetaAtDevice == null)
                    {
                        eMetaAtDevice = new EntityMetaAtDevice()
                        {
                            DeviceId = idDevice,
                            MetaId = guid.Value,
                        };
                        context.MetasAtDevice.Add(eMetaAtDevice);
                    }
                    eMetaAtDevice.PathAbs = meta.PathAbs;
                }
                else
                {
                    throw new FileNotFoundException(fiAsmdef.FullName);
                }
            }
            return await context.SaveChangesAsync();
        }
    }
}
