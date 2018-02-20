using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Owin.Sample.Controllers
{
    [Route("sample")]
    public class SampleController : Controller
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        [HttpGet("withparams/{x}/{y}")]
        public IEnumerable<string> Get(int x, string y)
        {
            return new[] { "value1", "value2" };
        }
    }
}
