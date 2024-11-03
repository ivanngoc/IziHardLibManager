using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;
using NuGet.Protocol;
using System.Text.Json;

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OldUnityAsmdefsController : ControllerBase
    {
        private readonly ModulesDbContextV1 _context;

        public OldUnityAsmdefsController(ModulesDbContextV1 context)
        {
            _context = context;
        }

        // GET: api/UnityAsmdefs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IziModelUnityAsmdef>>> GetUnityAsmdefs()
        {
            return await _context.UnityAsmdefs.ToListAsync();
        }

        // GET: api/UnityAsmdefs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IziModelUnityAsmdef>> GetIziModelUnityAsmdef(uint id)
        {
            var iziModelUnityAsmdef = await _context.UnityAsmdefs.FindAsync(id);

            if (iziModelUnityAsmdef == null)
            {
                return NotFound();
            }

            return iziModelUnityAsmdef;
        }

        // PUT: api/UnityAsmdefs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIziModelUnityAsmdef(uint id, IziModelUnityAsmdef iziModelUnityAsmdef)
        {
            if (id != iziModelUnityAsmdef.Id)
            {
                return BadRequest();
            }

            _context.Entry(iziModelUnityAsmdef).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IziModelUnityAsmdefExists(id))
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

        // POST: api/UnityAsmdefs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IziModelUnityAsmdef>> PostIziModelUnityAsmdef(IziModelUnityAsmdef iziModelUnityAsmdef)
        {
            _context.UnityAsmdefs.Add(iziModelUnityAsmdef);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIziModelUnityAsmdef", new { id = iziModelUnityAsmdef.Id }, iziModelUnityAsmdef);
        }

        // DELETE: api/UnityAsmdefs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIziModelUnityAsmdef(uint id)
        {
            var iziModelUnityAsmdef = await _context.UnityAsmdefs.FindAsync(id);
            if (iziModelUnityAsmdef == null)
            {
                return NotFound();
            }

            _context.UnityAsmdefs.Remove(iziModelUnityAsmdef);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool IziModelUnityAsmdefExists(uint id)
        {
            return _context.UnityAsmdefs.Any(e => e.Id == id);
        }

        [HttpGet("search/{substring}")]
        public async Task<IActionResult> PartialSearch(string substring)
        {
            var model = _context.UnityAsmdefs.FirstOrDefault(x => x.Guid.ToString().StartsWith(substring));
            if (model != null) return Content(JsonSerializer.Serialize(model));
            return Content("{}");
        }
    }
}
