using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Benning.Dev.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class HostController : ControllerBase, IDisposable
    {
        [HttpGet]
        [Produces("text/plain")]
        public IActionResult GetHostname() => Content(Environment.MachineName);

        [HttpGet]
        [Produces("text/plain")]
        [Route("ip-wan")]
        public async Task<IActionResult> GetWanIp()
        {
            var response = await _httpClient.GetAsync("http://ifconfig.me").ConfigureAwait(false);
            return response.IsSuccessStatusCode
                ? (IActionResult)Content(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                : (IActionResult)StatusCode((int)response.StatusCode);
        }

        [HttpGet]
        [Produces("text/plain")]
        [Route("ip-lan")]
        public IActionResult GetLanIp()
        {
            string ip;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "hostname",
                    Arguments = "-I",
                    RedirectStandardOutput = true
                };
                using var proc = Process.Start(startInfo);
                proc.WaitForExit(1000);
                ip = proc.StandardOutput.ReadToEnd();
            }
            else
            {
                ip = GetLocalIpAddress();
            }

            if (string.IsNullOrWhiteSpace(ip))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "The LAN IP address could not be resolved.");
            }
            else
            {
                return Content(ip);
            }
        }

        // Credit to https://www.c-sharpcorner.com/forums/how-to-get-local-machine-ip-address-in-net-core-22
        private static string GetLocalIpAddress()
        {
            UnicastIPAddressInformation preferredLanIp = null;
            foreach (var network in (NetworkInterface[])NetworkInterface.GetAllNetworkInterfaces())
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;

                var properties = network.GetIPProperties();
                if (properties.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    if (!address.IsDnsEligible)
                    {
                        if (preferredLanIp == null)
                            preferredLanIp = address;
                        continue;
                    }

                    // The best IP is the IP got from DHCP server
                    if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                    {
                        if (preferredLanIp?.IsDnsEligible != true)
                            preferredLanIp = address;
                        continue;
                    }
                    return address.Address.ToString();
                }
            }
            return preferredLanIp != null
                ? preferredLanIp.Address.ToString()
                : "";
        }

        private readonly HttpClient _httpClient;

        public HostController()
        {
            _httpClient = new HttpClient();
        }

        void IDisposable.Dispose() => _httpClient.Dispose();
    }
}
