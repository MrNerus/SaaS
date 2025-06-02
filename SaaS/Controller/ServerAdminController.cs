using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaaS.DTO;
using SaaS.Model;
using SaaS.Service;

namespace SaaS.Controller
{

    [ApiController]
    [Route("api")]
    public class ServerAdminController(ServerAdminService serverAdminService) : ControllerBase
    {
        public readonly ServerAdminService _serverAdminService = serverAdminService;

        [HttpPost("registerTenant")]
        [Authorize]
        public async Task<IActionResult> RegisterTenant(ConnectionModel connectionModel)
        {
            try
            {
                UserInstance userInstance = HttpContext.Items["UserInstance"] as UserInstance ?? throw new Exception("Your session has been expired. Please login again.");
                if (userInstance.UserType != "Console") throw new Exception("You are not authorized to register tenant");

                connectionModel.ConnectionName = connectionModel.ConnectionName.ToLower();

                await _serverAdminService.RegisterTenant(connectionModel, userInstance); ;
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
