using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Benning.Dev.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IpController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public IpController()
        {
            _httpClient = new HttpClient();
        }

        [HttpGet]
        [Produces("text/plain")]
        public async Task<IActionResult> Get()
        {
            var response = await _httpClient.GetAsync("http://ifconfig.me").ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? Content(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                : (IActionResult)StatusCode((int)response.StatusCode);
        }
    }
}
