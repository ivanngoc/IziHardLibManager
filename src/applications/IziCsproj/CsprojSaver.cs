using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IziLibrary.Database.DataBase.EfCore;

namespace IziHardGames.DotNetProjects
{
    public class CsprojSaver(IziProjectsDbContext context) : ICsprojSaver
    {
        public Task SaveToDbAsync(IEnumerable<FileInfo> fileInfos)
        {
            foreach (var fileInfo in fileInfos)
            {
                var meta = new Csproj(fileInfo);
                var projAtDevice = new CsProjectAtDevice()
                {
                    DeviceId = IziEnvironmentsHelper.
                };
                if (meta.TryGetGuid(out var guid))
                {
                    context.Csprojs.Add(new EntityCsproj()
                    {
                        EntityCsprojId = guid,
                    });
                }
            }
            throw new System.NotImplementedException();
        }
    }
}
