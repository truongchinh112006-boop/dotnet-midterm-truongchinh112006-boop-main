using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.Data; 
using Core.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // 1. (1.5đ) Bắt lỗi trùng Username - Trả về 400 BadRequest
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username đã tồn tại.");

            // 2. (1.5đ) Mã hóa mật khẩu bằng BCrypt trước khi lưu
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User 
            {
                Username = request.Username,
                PasswordHash = passwordHash, // Lưu dạng Hash
                Email = string.Empty
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Đăng ký thành công!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // 3. (1.0đ) Bảo mật Đăng nhập - So sánh Hash (Verify)
            // Nếu sai pass hoặc không thấy user thì return 401 Unauthorized
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Sai tên đăng nhập hoặc mật khẩu.");

            // 4. (2.0đ) Trả về JWT Token thật
            var token = CreateToken(user);
            return Ok(new { token = token });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Lấy Key bảo mật từ cấu hình
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["AppSettings:Token"] ?? "Secret_Key_Sieu_Bao_Mat_Min_32_Chars_2026"));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

    public record RegisterDto(string Username, string Password);
    public record LoginDto(string Username, string Password);
}