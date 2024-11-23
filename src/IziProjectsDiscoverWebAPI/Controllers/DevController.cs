using IziLibrary.Database.DataBase.EfCore;
using Microsoft.AspNetCore.Mvc;

namespace IziProjectsDiscoverWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevController(IziProjectsDbContext context) : ControllerBase
    {
        [HttpGet(nameof(EnsureDbCreated))]
        public async Task<IActionResult> EnsureDbCreated()
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            await context.Init();
            return NoContent();
        }
    }
}
