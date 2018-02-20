using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Sample.Controllers
{
    [Route("sampleignore")]
    public class SampleIgnoreController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "get";
        }
    }
}
