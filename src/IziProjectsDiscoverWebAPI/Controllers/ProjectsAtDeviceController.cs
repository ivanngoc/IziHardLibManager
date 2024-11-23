using IziLibrary.Database.DataBase.EfCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsAtDeviceController(IziProjectsDbContext context) : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetProjects(Guid device)
        {
            var q = context.Projects.Where(x => x.Devices.Any(y => x.Id == device));
            return Ok(await q.ToArrayAsync());
        }
    }
}
