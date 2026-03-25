using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.IntegrationTests;

public class ExamTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ExamTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        try { _client = factory.CreateClient(); } catch { /* Bỏ qua lỗi nếu app chưa build được */ }
    }

    private record RegisterReq(string Username, string Password, string Email);
    private record LoginReq(string Username, string Password);
    private record LoginRes(string Token);

    // ==========================================
    // PHẦN 1: TEST MÔI TRƯỜNG & CẤU HÌNH
    // ==========================================
    [Fact(DisplayName = "1. Môi trường: App khởi động thành công (Fix lỗi DbContext)")]
    public void Test_App_ShouldStart()
    {
        Assert.NotNull(_client);
    }

    [Fact(DisplayName = "2. Môi trường: Giao diện Scalar hoạt động")]
    public async Task Test_Scalar_UI_Loads()
    {
        var response = await _client.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString());
    }

    // ==========================================
    // PHẦN 2: TEST NGHIỆP VỤ (BUSINESS LOGIC)
    // ==========================================
    [Fact(DisplayName = "3. Nghiệp vụ: Đăng ký thành công (Happy Path)")]
    public async Task Test_Register_Success()
    {
        var req = new RegisterReq("user_happy", "Pass123!", "happy@test.com");
        var res = await _client.PostAsJsonAsync("/api/auth/register", req);
        Assert.True(res.IsSuccessStatusCode);
    }

    [Fact(DisplayName = "4. Nghiệp vụ: Báo lỗi 400 nếu Username đã tồn tại")]
    public async Task Test_Register_Duplicate_ReturnsBadRequest()
    {
        var req = new RegisterReq("duplicate_user", "Pass123!", "dup@test.com");
        await _client.PostAsJsonAsync("/api/auth/register", req); // Lần 1
        var res2 = await _client.PostAsJsonAsync("/api/auth/register", req); // Lần 2
        Assert.Equal(HttpStatusCode.BadRequest, res2.StatusCode);
    }

    [Fact(DisplayName = "5. Nghiệp vụ: Đăng nhập sai trả về 401 Unauthorized")]
    public async Task Test_Login_WrongPass_ReturnsUnauthorized()
    {
        var req = new RegisterReq("login_user", "CorrectPass", "log@test.com");
        await _client.PostAsJsonAsync("/api/auth/register", req);
        
        var badLogin = new LoginReq("login_user", "WrongPass");
        var res = await _client.PostAsJsonAsync("/api/auth/login", badLogin);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact(DisplayName = "6. Nghiệp vụ: Phải mã hóa (Hash) mật khẩu xuống DB")]
    public async Task Test_Password_MustBeHashed_InDatabase()
    {
        var req = new RegisterReq("hash_user", "MySecretP@ss", "hash@test.com");
        await _client.PostAsJsonAsync("/api/auth/register", req);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userInDb = db.Users.FirstOrDefault(u => u.Username == "hash_user");

        Assert.NotNull(userInDb);
        Assert.NotEqual("MySecretP@ss", userInDb.PasswordHash); // Không được lưu plain-text
    }

    [Fact(DisplayName = "7. Nghiệp vụ: Trả về chuẩn JWT Token khi Login")]
    public async Task Test_Login_Returns_Valid_JWT()
    {
        var req = new RegisterReq("jwt_user", "Pass123!", "jwt@test.com");
        await _client.PostAsJsonAsync("/api/auth/register", req);

        var loginReq = new LoginReq("jwt_user", "Pass123!");
        var res = await _client.PostAsJsonAsync("/api/auth/login", loginReq);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var data = await res.Content.ReadFromJsonAsync<LoginRes>();
        
        Assert.NotNull(data);
        Assert.False(string.IsNullOrEmpty(data.Token));
        
        // JWT Token luôn có cấu trúc 3 phần chia bởi dấu chấm
        Assert.Equal(2, data.Token.Count(c => c == '.')); 
    }
}
