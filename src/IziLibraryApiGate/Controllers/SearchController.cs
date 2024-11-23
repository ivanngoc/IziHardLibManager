using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IziHardGames.Libs.IziLibrary.Contracts;
using IziHardGames.Projects.DataBase;

namespace IziLibraryApiGate.Controllers
{
    /// <summary>
    /// Поиск проекта в БД
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ModulesDbContextV2 context;

        public SearchController(ModulesDbContextV2 context)
        {
            this.context = context;
        }

        //// GET: api/Search
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ModelAsmdef>>> GetAsmdefs()
        //{
        //    return await context.Asmdefs.ToListAsync();
        //}

        //// GET: api/Search/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<ModelAsmdef>> GetModelAsmdef(int id)
        //{
        //    var modelAsmdef = await context.Asmdefs.FindAsync(id);

        //    if (modelAsmdef == null)
        //    {
        //        return NotFound();
        //    }

        //    return modelAsmdef;
        //}

        //// PUT: api/Search/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutModelAsmdef(int id, ModelAsmdef modelAsmdef)
        //{
        //    if (id != modelAsmdef.Id)
        //    {
        //        return BadRequest();
        //    }

        //    context.Entry(modelAsmdef).State = EntityState.Modified;

        //    try
        //    {
        //        await context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ModelAsmdefExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/Search
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<ModelAsmdef>> PostModelAsmdef(ModelAsmdef modelAsmdef)
        //{
        //    context.Asmdefs.Add(modelAsmdef);
        //    await context.SaveChangesAsync();

        //    return CreatedAtAction("GetModelAsmdef", new { id = modelAsmdef.Id }, modelAsmdef);
        //}

        //// DELETE: api/Search/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteModelAsmdef(int id)
        //{
        //    var modelAsmdef = await context.Asmdefs.FindAsync(id);
        //    if (modelAsmdef == null)
        //    {
        //        return NotFound();
        //    }

        //    context.Asmdefs.Remove(modelAsmdef);
        //    await context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool ModelAsmdefExists(int id)
        //{
        //    return context.Asmdefs.Any(e => e.Id == id);
        //}

        [HttpGet("SearchEverywhere")]
        public IEnumerable<ProjectItem> SearchEverywhere([FromQuery] string substring)
        {
            IEnumerable<ProjectItem> result = Enumerable.Empty<ProjectItem>();
            //result = result.Concat(context.Asmdefs.Where(x => x.FileName.Contains(substring, StringComparison.InvariantCultureIgnoreCase)));
            result = result.Concat(context.Asmdefs.Where(x => EF.Functions.Like(x.FileName.ToLower(), $"%{substring}%".ToLower())));
            return result;
        }
    }
}
