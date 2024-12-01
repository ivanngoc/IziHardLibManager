using IziHardGames.Asmdefs.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsmdefsController(IAsmdefSearcher searcher) : ControllerBase
    {
        [HttpPost(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {
            var count = await searcher.DiscoverAndSaveAsync();
            return Ok(count);
        }
    }
}
