using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Benning.Dev.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DateTimeController : ControllerBase
    {
        [HttpGet]
        [Produces("text/plain")]
        public IActionResult Get() => Content(DateTimeOffset.UtcNow.ToString("u"));
    }
}
