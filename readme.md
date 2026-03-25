[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/ON8DfACh)
[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-2e0aaae1b6195c2367325f4f02e2d04e9abb55f0b24a779b69b11b9e10269abc.svg)](https://classroom.github.com/online_ide?assignment_repo_id=23273961&assignment_repo_type=AssignmentRepo)
# 🎯 ĐỀ THI GIỮA KỲ: FIX BUG DỰ ÁN USER MANAGEMENT (10 ĐIỂM)

Chào mừng các bạn đến với bài thi thực hành! Bạn vừa được tuyển vào làm Software Engineer tại một công ty công nghệ. Nhiệm vụ đầu tiên của bạn là tiếp nhận một dự án **Quản lý người dùng (User Management)** từ một nhân viên cũ vừa nghỉ việc. 

Dự án này được viết bằng **.NET 10** theo mô hình Clean Architecture, nhưng hiện tại nó đang chứa rất nhiều lỗi cấu hình và sai sót nghiêm trọng về nghiệp vụ (Business Logic). 

**Nhiệm vụ của bạn là: Sửa toàn bộ lỗi để vượt qua hệ thống chấm điểm tự động (Autograding) của Giảng viên.**

---

## 🛠 MÔI TRƯỜNG & CÔNG NGHỆ
- **Framework:** .NET 10.0
- **Database:** SQLite (Dùng EF Core)
- **Kiến trúc:** Core - Infrastructure - API - Tests
- **Công cụ API:** OpenAPI & Scalar

---

## 📝 DANH SÁCH NHIỆM VỤ (CÁC BUG CẦN FIX)

### PHẦN 1: SỬA LỖI MÔI TRƯỜNG & CẤU HÌNH (4.0 ĐIỂM)
Dự án hiện tại **không thể Build** và **không chạy được**. Bạn hãy tìm và sửa các lỗi trong `src/API/Program.cs` và các file `.csproj`:
1. **(1.0đ) Fix lỗi Build:** Dự án đang dùng thư viện Swagger cũ kĩ, hãy dọn dẹp nó và cài đặt `Microsoft.AspNetCore.OpenApi` chuẩn của .NET 10. Nhớ thêm `using Scalar.AspNetCore;`.
2. **(1.5đ) Fix lỗi Database:** EF Core chưa được cấu hình. Bạn cần cài gói `Microsoft.EntityFrameworkCore.Design` vào project API và đăng ký `AppDbContext` vào hệ thống Dependency Injection.
3. **(1.5đ) Khởi chạy UI:** Đảm bảo khi chạy ứng dụng, truy cập vào `http://localhost:<port>/scalar/v1` sẽ hiện lên giao diện tài liệu API.

### PHẦN 2: SỬA LỖI NGHIỆP VỤ - BUSINESS LOGIC (6.0 ĐIỂM)
Nhân viên cũ đã code phần `AuthController.cs` rất cẩu thả. Bạn hãy vào đó và sửa lại:

4. **(1.5đ) Bắt lỗi trùng lặp:** Hàm Register hiện đang cho phép tạo nhiều User có cùng Username. Hãy thêm code kiểm tra, nếu trùng phải trả về `400 Bad Request`.
5. **(1.5đ) Mã hóa mật khẩu:** Mật khẩu đang bị lưu dưới dạng Plain-text. Hãy cài thư viện (ví dụ: `BCrypt.Net-Next`) để băm (Hash) mật khẩu trước khi lưu xuống Database.
6. **(1.0đ) Bảo mật Đăng nhập:** Sửa lại hàm Login để so sánh mật khẩu bằng thuật toán Hash. Nếu sai pass trả về `401 Unauthorized`.
7. **(2.0đ) Trả về JWT Token:** Hàm Login đang trả về một chuỗi Fake. Hãy viết code để sinh ra một chuỗi JSON Web Token (JWT) hợp lệ.

*(💡 **Gợi ý:** Nếu bạn gặp khó khăn ở Phần 2, hãy mở file `HINTS.md` đính kèm trong thư mục này để xem phao cứu sinh nhé!)*

---

## 🚀 HƯỚNG DẪN NỘP BÀI

Hệ thống **GitHub Classroom** sẽ tự động chấm điểm bài của bạn mỗi khi bạn Push code lên. 

1. Mở Terminal tại thư mục gốc của dự án.
2. Thêm các file đã sửa:
   ```bash
   git add .
   ```
3. Tạo commit:
   ```bash
   git commit -m "Hoan thanh bai thi giua ky"
   ```
4. Đẩy code lên GitHub:
   ```bash
   git push origin main
