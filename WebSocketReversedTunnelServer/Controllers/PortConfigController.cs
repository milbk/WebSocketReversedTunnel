using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebSocketReversedTunnelServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PortConfigController : ControllerBase
    {

        public PortConfigController()
        {
        }
        // POST api/portconfig
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}