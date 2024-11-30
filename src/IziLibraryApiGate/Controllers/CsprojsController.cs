using Microsoft.AspNetCore.Mvc;
using IziHardGames.DotNetProjects;
using System.Diagnostics;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;

namespace IziLibraryApiGate.Controllers
{
    /// <summary>
    /// Discover All kind of projects at device
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CsprojsController(ICsproSearcher searcher, ICsprojProcessor processor, ICsprojSaver saver, IziProjectsDbContext context) : ControllerBase
    {
        /// <summary>
        /// Ищет все *.cspoj по дирикториям в настройках и добавляет в <PropertyGroup/> необходимые данные (автор, Guid проекта, компания)
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Ищет все *.cspoj по дирикториям в настройках и добавляет запись в БД: PostgreSQL | host.docker.internal:5432 | IziProjectsDbContext | public | Table: Csprojs 
        /// </summary>
        [HttpGet(nameof(DiscoverAndSaveToDb))]
        public async Task<IActionResult> DiscoverAndSaveToDb()
        {
            var csprojsFullPaths = searcher.FindMyCsprojs();
            var count = await saver.SaveToDbAsync(csprojsFullPaths);
            return Ok(count);
        }


        [HttpGet(nameof(FillRelationsAsParents))]
        public async Task<IActionResult> FillRelationsAsParents()
        {
            var count = await processor.FillRelationsAsParentsAsync();
            return Ok(count);
        }

        [HttpGet(nameof(FillRelationsAsChilds))]
        public async Task<IActionResult> FillRelationsAsChilds()
        {
            var count = await processor.FillRelationsAsChildsByIncludeFileExistingAsync();
            return Ok(count);
        }

        [HttpGet(nameof(FillRelationsAsChildsByCsprojFileNameAsync))]
        public async Task<IActionResult> FillRelationsAsChildsByCsprojFileNameAsync()
        {
            var count = await processor.FillRelationsAsChildsByCsprojFileNameAsync();
            return Ok(count);
        }

        [HttpGet(nameof(GetRelationsWithoutChilds))]
        public async Task<IActionResult> GetRelationsWithoutChilds()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var atDevice = await context.Relations.Where(x => x.ChildId == null).SelectMany(x => x.RelationsAtDevice.Select(x => new
            {
                PathParent = x.Relation.Parent.CsProjectAtDevices.First(x => x.DeviceId == idDevice).PathAbs,
                PathChild = x.Relation.Child.CsProjectAtDevices.First(x => x.DeviceId == idDevice).PathAbs,
                x.Include,
                x.DeviceId,

            })).Where(x => x.DeviceId == idDevice).ToArrayAsync();
            return Ok(atDevice);
        }

        [HttpGet(nameof(ReplaceChildInclude))]
        public async Task<IActionResult> ReplaceChildInclude([FromQuery] string find, [FromQuery] string replace)
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var count = await processor.ReplaceChildIncludeAsync(find, replace);
            return Ok(count);
        }
        /// <summary>
        /// Добавляет {ProjectReference}.{Project}=GUID и делает {ProjectReference.Include}=PathAbs
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(FormatDependecies))]
        public async Task<IActionResult> FormatDependecies()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var count = await processor.FormatDependecies();
            return Ok(count);
        }

        [HttpGet(nameof(DistinctRelations))]
        public async Task<IActionResult> DistinctRelations()
        {
            var count = await processor.DistinctRelationsAsync();
            return Ok(count);
        }

        [HttpGet(nameof(FormatIncludePathToEnvVarBasedPath))]
        public async Task<IActionResult> FormatIncludePathToEnvVarBasedPath()
        {
            var count = await processor.FormatIncludePathToEnvVarBasedPathAsync();
            return Ok(count);
        }

        /// <summary>
        /// По всем <see cref="EntityCsproj"/> заменяет {ProjectReference}.Include с абсолютного пути сначала на ENV-VAR based, потом оставшиеся на относительный путь
        /// </summary>
        /// <returns></returns>
        [HttpGet(nameof(ReplaceAbsIncludesWithEnvBasedAndRelative))]
        public async Task<IActionResult> ReplaceAbsIncludesWithEnvBasedAndRelative()
        {
            var count0 = await processor.ReplaceAbsIncludesWithEnvBasedAsync();
            var count1 = await processor.ReplaceAbsIncludesWithRelativeAsync();
            return Ok(count0 + count1);
        }
    }
}
