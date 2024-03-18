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
            using ModulesDbContext context = new ModulesDbContext();
            return View(context.Csprojs.Include(x => x.Module).ToArray());
        }

        public ActionResult Connections()
        {
            using ModulesDbContext context = new ModulesDbContext();
            return View(context.Relations.Include(x => x.From).Include(x => x.To).ToArray());
        }
    }
}