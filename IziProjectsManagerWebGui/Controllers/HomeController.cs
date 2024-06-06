using System.Diagnostics;
using IziProjectsManagerWebGui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;

namespace IziProjectsManagerWebGui.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Главная страница
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns Homepage</response>
        /// <response code="400">Error</response>
		[HttpGet]
        [Route("")]
        [Produces("text/html")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        //[ProducesResponseType(typeof(HomeController), StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(HomeController), StatusCodes.Status202Accepted, "description?", "some parameter2")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}