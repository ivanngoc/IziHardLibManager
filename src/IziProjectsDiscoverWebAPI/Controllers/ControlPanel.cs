using IziHardGames.IziLibrary.ForCsproj;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IziProjectsDiscoverWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlPanel(Dicsover dicsover, IziProjectsDbContext context) : ControllerBase
    {
        [HttpGet("Discover")]
        public async Task<IActionResult> Discover()
        {
            Guid deviceGuid = IziProjectsDbContext.laptop;
            var q = context.DeviceSettings.Where(x => x.Id == deviceGuid).Include(x => x.Device);
            var s = await q.FirstOrDefaultAsync();
            var fis = dicsover.FindCsProjAsync(s.SourceDirs);
            var result = new List<CsProjectAtDevice>();

            await foreach (var fi in fis)
            {
                var proj = new CsProjectAtDevice()
                {
                    Id = default,
                    Device = s.Device,
                    DeviceId = s.Device.Id,
                    PathAbs = fi.FullName,
                };
                result.Add(proj);
                context.Add(proj);
                Console.WriteLine(fi.FullName);
            }
            await context.SaveChangesAsync();
            return Ok(result);
        }
    }
}
