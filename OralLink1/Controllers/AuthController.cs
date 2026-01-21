using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OralLink1.Models;
using OralLink1.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace OralLink1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

       
        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger,
            JwtService jwtService)
        {
            _userService = userService;
            _logger = logger;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"Login attempt: {request.Email}");

              
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Email and password are required"
                    });
                }

                var user = await _userService.GetUserByEmail(request.Email);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {request.Email}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var authenticatedUser = await _userService.AuthenticateUser(request.Email, request.Password);

                if (authenticatedUser == null)
                {
                    _logger.LogWarning($"Wrong password for: {request.Email}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid password"
                    });
                }

                if (authenticatedUser.Role == "Student" &&
                   !Regex.IsMatch(authenticatedUser.Email, @"@o6u\.edu\.eg$", RegexOptions.IgnoreCase))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Students must use O6U email"
                    });
                }

                _logger.LogInformation($"Login successful - {authenticatedUser.Role}: {authenticatedUser.Name}");

                var token = _jwtService.GenerateToken(authenticatedUser);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    User = new UserData
                    {
                        Id = authenticatedUser.Id,
                        Name = authenticatedUser.Name,
                        Role = authenticatedUser.Role,
                        Email = authenticatedUser.Email
                    },
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error: {request.Email}");
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("protected")]
        [Authorize]  
        public IActionResult Protected()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "This is a protected endpoint!",
                userId = userId,
                userName = userName,
                userRole = userRole,
                timestamp = DateTime.Now
            });
        }

    }
}