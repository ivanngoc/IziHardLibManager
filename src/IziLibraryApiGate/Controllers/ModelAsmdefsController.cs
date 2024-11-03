using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IziHardGames.Libs.IziLibrary.Contracts;
using IziHardGames.Projects.DataBase;
using System.Text.Json;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelAsmdefsController : ControllerBase
    {
        private readonly ModulesDbContextV2 _context;

        public ModelAsmdefsController(ModulesDbContextV2 context)
        {
            _context = context;
        }

        // GET: api/ModelAsmdefs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelAsmdef>>> GetAsmdefs()
        {
            return await _context.Asmdefs.ToListAsync();
        }

        // GET: api/ModelAsmdefs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ModelAsmdef>> GetModelAsmdef(int id)
        {
            var modelAsmdef = await _context.Asmdefs.FindAsync(id);

            if (modelAsmdef == null)
            {
                return NotFound();
            }

            return modelAsmdef;
        }

        // PUT: api/ModelAsmdefs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModelAsmdef(int id, ModelAsmdef modelAsmdef)
        {
            if (id != modelAsmdef.Id)
            {
                return BadRequest();
            }

            _context.Entry(modelAsmdef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModelAsmdefExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ModelAsmdefs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ModelAsmdef>> PostModelAsmdef(ModelAsmdef modelAsmdef)
        {
            _context.Asmdefs.Add(modelAsmdef);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetModelAsmdef", new { id = modelAsmdef.Id }, modelAsmdef);
        }

        // DELETE: api/ModelAsmdefs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModelAsmdef(int id)
        {
            var modelAsmdef = await _context.Asmdefs.FindAsync(id);
            if (modelAsmdef == null)
            {
                return NotFound();
            }

            _context.Asmdefs.Remove(modelAsmdef);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("search/{substring}")]
        public async Task<IActionResult> PartialSearch(string substring)
        {
            var model = _context.Asmdefs.Where(x => x.Guid.ToString().StartsWith(substring));
            if (model != null) return Content(JsonSerializer.Serialize(model));
            return Content("[]");
        }

        private bool ModelAsmdefExists(int id)
        {
            return _context.Asmdefs.Any(e => e.Id == id);
        }
    }
}
