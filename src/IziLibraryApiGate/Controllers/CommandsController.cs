using IziHardGames.IziLibrary.Commands.AtDataBase;
using IziHardGames.Projects.DataBase;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IziLibraryApiGate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private FillDatabaseWithAsmdef fillAsmdef;
        private ModulesDbContextV2 context;

        public CommandsController(FillDatabaseWithAsmdef fillAsmdef, ModulesDbContextV2 context)
        {
            this.fillAsmdef = fillAsmdef;
            this.context = context;
        }
        // GET: api/<CommandsController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CommandsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CommandsController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] string value)
        {
            await fillAsmdef.FillDatabase(default).ConfigureAwait(false);
            return Created(string.Empty, context.Asmdefs.ToArray());
        }

        // PUT api/<CommandsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CommandsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
