using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TokenApi.Models;
using BCrypt.Net;

namespace TokenApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Kullanıcı Kaydı (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            if (await _context.Users.AnyAsync(x => x.Username == registerModel.Username))
            {
                return BadRequest("Bu Kullanıcı Adına ait bir kullanıcı Zaten Var");
            }

            if (registerModel.Password != registerModel.PasswordConfirm)
            {
                return BadRequest("Girilen Şifreler Eşleşmiyor");
            }

            var user = new User
            {
                Username = registerModel.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(registerModel.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kullanıcı Kaydı Başarıyla Gerçekleştirildi");
        }


        // Kullanıcı Girişi (Login) ve Token Alma
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == login.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return Unauthorized("Geçersiz Kullanıcı Bilgileri");
            }

            var token = TokenOlustur(user.Username);
            return Ok(new { Token = token });
        }

        // Token Yenileme (Refresh Token)
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string expiredToken)
        {
            var principal = GetPrincipalFromExpiredToken(expiredToken);
            if (principal == null)
            {
                return BadRequest("Geçersiz token");
            }

            var username = principal.Identity?.Name;
            if (username == null)
            {
                return BadRequest("Geçersiz token");
            }

            var newToken = TokenOlustur(username);
            return Ok(new { Token = newToken });
        }

        // Token Oluşturmaya yardımcı mettot
        private string TokenOlustur(string username)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, username)
                },
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Süresi Geçmiş Token'dan bilgi alma
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // Süresi geçmiş token'ları kabul et
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}