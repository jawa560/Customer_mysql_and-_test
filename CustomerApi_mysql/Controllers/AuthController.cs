using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CustomerApi.data;
using CustomerApi.Data;

namespace CustomerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == login.Username && u.Password == login.Password);

            if (user != null)
            {
                var old_token = await _context.Tokens.SingleOrDefaultAsync(t => t.Username == login.Username);

                if (old_token != null)
                {
                    _context.Tokens.Remove(old_token);
                    await _context.SaveChangesAsync();

                }

                var tokenValue = GenerateJwtToken(user);
                var token = new Token
                {
                    Username = user.Username,
                    TokenValue = tokenValue,
                    ExpiryDate = DateTime.Now.AddMinutes(60)
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                return Ok(new { Token = tokenValue });
            }

            return Unauthorized();
        }

        [HttpPost("logout")]
        //public async Task<IActionResult> Logout([FromBody] LogoutModel logout)
        //{
        //    var token = await _context.Tokens.SingleOrDefaultAsync(t => t.TokenValue == logout.Token);

        //    if (token != null)
        //    {
        //        _context.Tokens.Remove(token);
        //        await _context.SaveChangesAsync();
        //        return Ok();
        //    }

        //    return BadRequest("Invalid token");
        //}
        //public async Task<IActionResult> Logout([FromBody] LogoutModel logout)
        //{
        //    var token = await _context.Tokens.SingleOrDefaultAsync(t => t.TokenValue == logout.Token);

        //    if (token != null)
        //    {
        //        _context.Tokens.Remove(token);
        //        await _context.SaveChangesAsync();
        //        return Ok();
        //    }

        //    return BadRequest("Invalid token");
        //}
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var token = await _context.Tokens.SingleOrDefaultAsync(t => t.Username == username);

            if (token != null)
            {
                _context.Tokens.Remove(token);
                await _context.SaveChangesAsync();
                return Ok();
            }

            return BadRequest("Invalid token");
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LogoutModel
    {
        public string Token { get; set; }
    }
}
