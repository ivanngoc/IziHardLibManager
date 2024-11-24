using Microsoft.AspNetCore.Mvc;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsmdefsController : ControllerBase
    {
        [HttpPost(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {

        }
    }
}
