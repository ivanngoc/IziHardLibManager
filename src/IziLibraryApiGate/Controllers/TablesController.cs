using Microsoft.AspNetCore.Mvc;
using IziLibrary.Database.DataBase.EfCore;
using IziHardGames.Projects.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using IziHardGames.IziProjectsManager.Common.Dtos;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TablesController(IziProjectsDbContext context) : ControllerBase
    {

        [HttpGet(nameof(Csprojs))]
        public async Task<IActionResult> Csprojs()
        {
            var q = context.Csprojs.AsNoTracking().Include(x => x.CsProjectAtDevices).ThenInclude(x => x.Device);
            var csprojs = await q.ToArrayAsync();

            var result = csprojs.Select(x => new CsprojDto()
            {
                Guid = x.EntityCsprojId,
                Description = x.Description,
                Name = x.CsProjectAtDevices.FirstOrDefault()?.GetFileName(),
                Paths = x.CsProjectAtDevices.Select(x=>x.PathAbs).ToArray(),
                Devices = x.CsProjectAtDevices.Select(x => x.Device).Select(x => new DeviceDto()
                {
                    Guid = x.Id,
                    Description = x.Description,
                }).DistinctBy(x => x.Guid).ToArray(),

            });
            return Ok(result);
        }
    }
}
