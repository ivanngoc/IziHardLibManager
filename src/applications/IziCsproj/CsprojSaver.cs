using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.DotNetProjects
{
    public class CsprojSaver(IziProjectsDbContext context) : ICsprojSaver
    {
        public async Task<int> SaveToDbAsync(IEnumerable<FileInfo> fileInfos)
        {
            foreach (var fileInfo in fileInfos)
            {
                var meta = new Csproj(fileInfo);

                if (meta.TryGetGuid(out var guid))
                {
                    var id = (CsprojId)guid;
                    var toProcess = await context.Csprojs.Where(x => x.EntityCsprojId == id).Include(x => x.CsProjectAtDevices).FirstOrDefaultAsync();

                    if (toProcess == null)
                    {
                        toProcess = new EntityCsproj()
                        {
                            EntityCsprojId = id,
                        };
                        var projAtDevice = new CsProjectAtDevice()
                        {
                            DeviceId = IziEnvironmentsHelper.GetCurrentDeviceGuid(),
                            EntityCsprojId = id,
                            EntityCsproj = toProcess,
                            PathAbs = meta.FilePathAbsolute,
                        };
                        context.Csprojs.Add(toProcess);
                    }
                }
            }
            return await context.SaveChangesAsync();
        }
    }
}
