using Microsoft.AspNetCore.Mvc;
using Core.Entities;
using Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    public record RegisterDto(string Username, string Password, string Email);
    public record LoginDto(string Username, string Password);

    [HttpPost("register")]
    public IActionResult Register(RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            return BadRequest();
        }

        bool isExists = _context.Users.Any(u => u.Username == dto.Username);
        if (isExists)
        {
            return BadRequest("Username đã tồn tại");
        }

        string hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = hashedPass,
            Email = dto.Email
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(user);
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
        {
            return Unauthorized();
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
        if (user == null)
        {
            return Unauthorized();
        }

        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordCorrect)
        {
            return Unauthorized();
        }

        var token = GenerateJwtToken();

        return Ok(new { token });
    }

    private string GenerateJwtToken()
    {
        var secretKey = "Day_La_Mot_Chuoi_Bi_Mat_Sieu_Dai_Va_An_Toan_123456789!!!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}