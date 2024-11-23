using IziHardGames.FileSystem.NetCore;
using System.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.DotNetProjects;

namespace IziLibraryApiGate.Controllers
{
    /// <summary>
    /// Discover All kind of projects at device
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiscoverProjectsController(ICsproSearcher searcher) : ControllerBase
    {
        [HttpGet(nameof(DiscoverCsprojs))]
        public async Task<IActionResult> DiscoverCsprojs()
        {
            var csprojsFullPaths = searcher.FindMyCsprojs();
            var result = csprojsFullPaths.ToArray();
            return Ok(result);
        }
    }
}
