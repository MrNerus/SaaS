using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaaS.DTO;
using SaaS.Model;
using SaaS.Service;

namespace SaaS.Controller
{

    [ApiController]
    [Route("api/serveradmin")]
    public class ServerAdminController(ServerAdminService serverAdminService) : ControllerBase
    {
        public readonly ServerAdminService _serverAdminService = serverAdminService;

        [HttpPost("registerTenant")]
        public async Task<IActionResult> RegisterTenant(ConnectionModel connectionModel)
        {
            try
            {
                await _serverAdminService.RegisterTenant(connectionModel); ;
                return Ok("Ok.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("registerUser")]
        public async Task<IActionResult> RegisterUser(LoginDTO loginDTO)
        {
            try
            {
                await _serverAdminService.RegisterUser(loginDTO); ;
                return Ok("Ok.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("registerTanentServer")]
        public async Task<IActionResult> RegisterTanentServer(TenantServerRegistrationDTO serverRegistrationDTO)
        {
            try
            {
                string connectionString = await _serverAdminService.RegisterTanentServer(serverRegistrationDTO);;
                return Ok(connectionString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
