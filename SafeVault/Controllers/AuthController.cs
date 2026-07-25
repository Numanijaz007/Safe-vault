using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SafeVault.Models;
using SafeVault.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SafeVault.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<VaultUser> userManager;
        private readonly IConfiguration config;

        public AuthController(UserManager<VaultUser> userManager, IConfiguration config)
        {
            this.userManager = userManager;
            this.config = config;
        }

        public class RegisterRequest
        {
            public string Username { get; set; } = "";
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        public class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // extra validation on top of the model binding, since username
            // ends up used in a few places we don't want garbage in there
            if (!InputValidator.IsValidUsername(request.Username))
                return BadRequest("Username has to be 3-50 chars, letters/numbers/._- only.");

            var user = new VaultUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            // UserManager handles password hashing internally, we never touch
            // or store the raw password ourselves
            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await userManager.AddToRoleAsync(user, "User");

            return Ok("account created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized("invalid username or password");

            var passwordOk = await userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordOk)
                return Unauthorized("invalid username or password");

            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var keyString = config["Jwt:Key"] ?? throw new Exception("Jwt:Key missing from config");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
