using Microsoft.AspNetCore.Mvc;
using IziHardGames.DotNetProjects;

namespace IziLibraryApiGate.Controllers
{
    /// <summary>
    /// Discover All kind of projects at device
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CsprojsController(ICsproSearcher searcher, ICsprojProcessor processor, ICsprojSaver saver) : ControllerBase
    {
        [HttpGet(nameof(DiscoverAndEnsureRequiredMetas))]
        public async Task<IActionResult> DiscoverAndEnsureRequiredMetas()
        {
            var csprojsFullPaths = searcher.FindMyCsprojs();
            foreach (var csproj in csprojsFullPaths)
            {
                var temp = new Csproj(csproj);
                await processor.EnsureRequiredMetasAsync(temp);
            }
            return Ok(csprojsFullPaths.Select(x => x.FullName));
        }

        [HttpGet(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {
            var csprojsFullPaths = searcher.FindMyCsprojs();
            await saver.SaveToDbAsync(csprojsFullPaths);
            return Ok();
        }
    }
}
