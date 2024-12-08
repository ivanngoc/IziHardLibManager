using IziHardGames.Asmdefs.Contracts;
using Metas.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetasController(IMetaSearcher searcher) : ControllerBase
    {
        [HttpPost(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {
            var count = await searcher.DiscoverLinkedToAsmdefsAndSaveAsync();
            return Ok(count);
        }
    }
}
