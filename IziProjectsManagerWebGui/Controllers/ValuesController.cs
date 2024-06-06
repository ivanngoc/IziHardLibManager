using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IziProjectsManagerWebGui.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Route("GetSomething")]
        public string GetSomething() => GetType().AssemblyQualifiedName;              
        
    }
}
