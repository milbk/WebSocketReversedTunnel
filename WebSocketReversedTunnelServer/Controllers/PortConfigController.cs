using Microsoft.AspNetCore.Mvc;

namespace WebSocketReversedTunnelServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PortConfigController : ControllerBase
    {
        // POST api/portconfig
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}