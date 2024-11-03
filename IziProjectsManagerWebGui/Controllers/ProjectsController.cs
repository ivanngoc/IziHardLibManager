using System.Linq;
using IziHardGames.Projects.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IziProjectsManagerWebGui.Controllers
{
    public class ProjectsController : Controller
    {
        public ProjectsController()
        {

        }
        public ActionResult Csprojs()
        {
            using ModulesDbContextV1 context = new ModulesDbContextV1();
            return View(context.Csprojs.Include(x => x.Module).ToArray());
        }

        [HttpGet]
        [Route("Connections")]
        public ActionResult Connections()
        {
            using ModulesDbContextV1 context = new ModulesDbContextV1();
            return View(context.Relations.Include(x => x.From).Include(x => x.To).ToArray());
        }
    }
}