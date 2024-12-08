using IziHardGames.Asmdefs;
using IziHardGames.Asmdefs.Contracts;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsmdefsController(IAsmdefSearcher searcher, IziProjectsDbContext context) : ControllerBase
    {
        [HttpPost(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {
            var count = await searcher.DiscoverAndSaveAsync();
            return Ok(count);
        }

        [HttpPost(nameof(Find))]
        public async Task<IActionResult> Find(string id)
        {
            var guid = (AsmdefId)Guid.Parse(id);
            if (guid.Guid == Guid.Empty) throw new ArgumentException(id);
            var q = context.Asmdefs.Include(x => x.AsmdefsAtDevice).Where(x => x.EntityAsmdefId == guid);
            return Ok(await q.FirstOrDefaultAsync());
        }
    }
}
