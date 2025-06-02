using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaaS.DTO;
using SaaS.Model;
using SaaS.Service;

namespace SaaS.Controller
{

    [ApiController]
    public class DashboardControler(DashboardService dashboardService) : ControllerBase
    {
        public readonly DashboardService _dashboardService = dashboardService;

        [HttpPost("api/dashboard")]
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                UserInstance userInstance = HttpContext.Items["UserInstance"] as UserInstance ?? throw new Exception("Your session has been expired. Please login again.");

                if (userInstance.UserType == "Tenant") return Ok($"You have logged in as Tenant on {userInstance.ConnectionName}. Every task that you will do will be applied on {userInstance.ConnectionString} connection");
                else return Ok($"You have logged in as Console. Every task that you will do will be applied on config connection");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
