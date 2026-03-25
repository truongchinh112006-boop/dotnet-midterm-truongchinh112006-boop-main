# 💡 TÀI LIỆU GỢI Ý (HINTS) - SỬA LỖI NGHIỆP VỤ API

Chào các bạn sinh viên, nếu các bạn đang bị "mắc kẹt" ở các bài Test từ số 4 đến số 7 (phần Nghiệp vụ Business Logic) và chưa biết bắt đầu từ đâu, hãy tham khảo các gợi ý dưới đây nhé! 

*Lưu ý: Đây chỉ là định hướng tư duy và các đoạn code mẫu, bạn cần áp dụng linh hoạt vào `AuthController` của mình.*

---

### Gợi ý cho Test 4 & 5: Kiểm tra trùng lặp Username (Duplicate Check)
**Vấn đề:** Hiện tại API đang cho phép tạo ra N tài khoản có cùng Username, điều này làm hỏng tính toàn vẹn của hệ thống.

**Cách giải quyết:** Trước khi gọi lệnh `_context.Users.Add()`, bạn cần truy vấn xem Username đó đã tồn tại trong Database chưa.

**Gợi ý Code (Entity Framework Core):**
```csharp
// Sử dụng hàm Any() để kiểm tra sự tồn tại (Trả về true/false)
bool isExists = _context.Users.Any(u => u.Username == dto.Username);

if (isExists) 
{
    // Nếu đã tồn tại, lập tức chặn lại và trả về mã lỗi 400 Bad Request
    return BadRequest("Username này đã có người sử dụng!");
}
```

### Gợi ý cho Test 6: Băm mật khẩu (Password Hashing)
**Vấn đề:** Lưu mật khẩu dưới dạng chữ thô (plain-text) là một "tội ác" trong bảo mật. Nếu Database bị hack, toàn bộ mật khẩu của người dùng sẽ bị lộ.

**Cách giải quyết:** Không tự viết thuật toán mã hóa! Hãy sử dụng thư viện chuyên dụng như BCrypt.

**Bước 1: Cài đặt thư viện**
Mở Terminal, đứng ở thư mục gốc và cài package này vào project API:

```Bash
dotnet add src/API package BCrypt.Net-Next
```
**Bước 2: Sử dụng trong Đăng ký (Register)**
Thay vì gán trực tiếp PasswordHash = dto.Password, hãy băm nó:

```C#
// Băm mật khẩu của người dùng trước khi lưu
string hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);
var user = new User { 
    Username = dto.Username, 
    PasswordHash = hashedPass, // Lưu chuỗi đã băm!
    Email = dto.Email 
};
```
**Bước 3: Sử dụng trong Đăng nhập (Login)**
Không dùng == để so sánh mật khẩu nữa. Bạn phải lấy User từ DB ra, sau đó dùng hàm Verify để kiểm tra:

```C#
var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
if (user == null) return Unauthorized("Tài khoản không tồn tại");

// So sánh mật khẩu người dùng nhập (dto.Password) với mã băm trong DB (user.PasswordHash)
bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
if (!isPasswordCorrect) return Unauthorized("Sai mật khẩu");
```

### Gợi ý cho Test 7: Tạo chuỗi JWT Token (JSON Web Token)
**Vấn đề:** Hàm Login hiện tại đang trả về một chuỗi string giả mạo, Front-end không thể dùng nó để xác thực được.

**Cách giải quyết:** Bạn phải dùng các class có sẵn của .NET để "nhào nặn" ra một JWT Token gồm 3 phần chuẩn quốc tế.

Các bước tạo JWT trong AuthController:
Bạn sẽ cần dùng các thư viện: using System.IdentityModel.Tokens.Jwt;, using Microsoft.IdentityModel.Tokens;, và using System.Text;.

```C#
// 1. Khai báo một chuỗi bí mật (Secret Key) để ký Token. 
// Đáng lẽ cái này phải để trong appsettings.json, nhưng để test nhanh bạn có thể hardcode 1 chuỗi dài hơn 32 ký tự.
var secretKey = "Day_La_Mot_Chuoi_Bi_Mat_Sieu_Dai_Va_An_Toan_123456789!!!";
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

// 2. Chọn thuật toán mã hóa
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

// 3. (Tùy chọn) Thêm thông tin (Claims) vào Token, ví dụ: Id, Username
// var claims = new[] { new Claim(ClaimTypes.Name, user.Username) };

// 4. Cấu hình Token (Thời gian sống, người gửi, v.v.)
var tokenDescriptor = new SecurityTokenDescriptor
{
    // Subject = new ClaimsIdentity(claims),
    Expires = DateTime.UtcNow.AddHours(2), // Sống 2 tiếng
    SigningCredentials = credentials
};

// 5. Xuất xưởng Token
var tokenHandler = new JwtSecurityTokenHandler();
var token = tokenHandler.CreateToken(tokenDescriptor);
var tokenString = tokenHandler.WriteToken(token); // Đây chính là chuỗi eYJ... bạn cần trả về

return Ok(new { Token = tokenString });
```
Chúc các bạn debug thành công và đạt trọn vẹn 10 điểm!
