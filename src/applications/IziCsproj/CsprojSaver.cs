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
            int count = default;
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            foreach (var fileInfo in fileInfos)
            {
                var meta = new Csproj(fileInfo);

                if (meta.TryGetGuid(out var guid))
                {
                    Console.WriteLine($"{guid}: {fileInfo.FullName}");
                    if (guid == default) throw new FormatException(fileInfo.FullName);
                    var id = (CsprojId)guid;
                    var toProcess = await context.Csprojs.Where(x => x.EntityCsprojId == id).Include(x => x.CsProjectAtDevices).FirstOrDefaultAsync();
                    CsProjectAtDevice? csProjectAtDevice = null;
                    if (toProcess == null)
                    {
                        toProcess = new EntityCsproj()
                        {
                            EntityCsprojId = id,
                            CsProjectAtDevices = new List<CsProjectAtDevice>(),
                        };
                        csProjectAtDevice = new CsProjectAtDevice()
                        {
                            DeviceId = idDevice,
                            EntityCsproj = toProcess,
                            EntityCsprojId = id,
                        };
                        toProcess.CsProjectAtDevices.Add(csProjectAtDevice);
                        context.Csprojs.Add(toProcess);
                    }
                    if (csProjectAtDevice == null)
                    {
                        csProjectAtDevice = toProcess.CsProjectAtDevices.FirstOrDefault(x => x.DeviceId == idDevice);
                    }
                    ArgumentNullException.ThrowIfNull(csProjectAtDevice);
                    csProjectAtDevice.PathAbs = meta.FilePathAbsolute;
                }
            }
            count += await context.SaveChangesAsync();
            return count;
        }
    }
}
