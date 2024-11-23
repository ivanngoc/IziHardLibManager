using Microsoft.AspNetCore.Mvc;
using IziLibrary.Database.DataBase.EfCore;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevController(IziProjectsDbContext context) : ControllerBase
    {
        [HttpGet(nameof(EnsureDbContext))]
        public async Task<IActionResult> EnsureDbContext()
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            await context.Init();
            return NoContent();
        }
    }
}
