using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaaS.DTO;
using SaaS.Model;
using SaaS.Service;

namespace SaaS.Controller
{

    [ApiController]
    public class LoginController(LoginService loginService) : ControllerBase
    {
        public readonly LoginService _loginService = loginService;

        [HttpPost("api/login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                UserProfile userProfile = await _loginService.Login(loginDTO);
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("api/ConsoleLogin")]
        public async Task<IActionResult> ConsoleLogin(LoginDTO loginDTO)
        {
            try
            {
                UserProfile userProfile = await _loginService.ConsoleLogin(loginDTO);
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("api/logout")]
        public async Task<IActionResult> Logout(LoginDTO loginDTO)
        {
            try
            {
                UserProfile userProfile = await _loginService.Login(loginDTO);
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
