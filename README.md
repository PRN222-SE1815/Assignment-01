# 🏫 School Management System

Hệ thống quản lý trường học được xây dựng với ASP.NET Core MVC theo kiến trúc 3 lớp.

---

## 📋 Mục lục

- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cài đặt](#-cài-đặt)
- [Cấu hình](#-cấu-hình)
- [Chạy ứng dụng](#-chạy-ứng-dụng)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Tính năng](#-tính-năng)

---

## 💻 Yêu cầu hệ thống

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express hoặc Developer Edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

---

## 🚀 Cài đặt

### 1. Clone repository

```bash
git clone <repository-url>
cd Assignment-01
```

### 2. Restore packages

```bash
dotnet restore
```

### 3. Tạo Database

Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio** và chạy file SQL:

```sql
-- Chạy file PRN222_G5.sql để tạo database và seed data
```

Hoặc sử dụng command line:

```bash
sqlcmd -S . -i PRN222_G5.sql
```

> **Lưu ý:** File `PRN222_G5.sql` sẽ tạo database `SchoolManagement` và các bảng cần thiết.

---

## ⚙️ Cấu hình

### 1. Connection String

Mở file `Web/appsettings.json` và cập nhật connection string phù hợp với SQL Server của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SchoolManagement;User Id=sa;Password=YOUR_PASSWORD;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Các tùy chọn Connection String:**

| Loại kết nối | Connection String                                                                                   |
| ------------ | --------------------------------------------------------------------------------------------------- |
| Windows Auth | `Server=.;Database=SchoolManagement;Trusted_Connection=True;TrustServerCertificate=True;`           |
| SQL Auth     | `Server=.;Database=SchoolManagement;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;` |
| LocalDB      | `Server=(localdb)\\mssqllocaldb;Database=SchoolManagement;Trusted_Connection=True;`                 |

### 2. Cấu hình Email (SMTP) - Tùy chọn

Để sử dụng tính năng Forgot Password, cấu hình SMTP trong `appsettings.json`:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "your-email@gmail.com",
    "FromName": "SchoolManagement"
  }
}
```

> **Lưu ý:** Với Gmail, bạn cần tạo [App Password](https://myaccount.google.com/apppasswords) (yêu cầu bật 2FA).

### 3. Cấu hình AI (Gemini) - Tùy chọn

Nếu sử dụng tính năng AI Chat, thêm cấu hình Gemini API:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

---

## ▶️ Chạy ứng dụng

### Sử dụng .NET CLI

```bash
cd Web
dotnet run
```

Hoặc với hot reload:

```bash
dotnet watch run
```

### Sử dụng Visual Studio

1. Mở file `Assignment01_SchoolManagement.slnx`
2. Set `Web` là Startup Project
3. Nhấn `F5` hoặc `Ctrl+F5` để chạy

### Truy cập ứng dụng

- **URL:** https://localhost:5001 hoặc http://localhost:5000
- **Login Page:** https://localhost:5001/Auth/Login

---

## 📁 Cấu trúc dự án

```
Assignment-01/
├── Web/                          # Presentation Layer (ASP.NET Core MVC)
│   ├── Controllers/              # MVC Controllers
│   ├── Views/                    # Razor Views (.cshtml)
│   ├── Models/                   # ViewModels
│   ├── Hubs/                     # SignalR Hubs
│   ├── wwwroot/                  # Static files (CSS, JS)
│   └── Program.cs                # Entry point & DI configuration
│
├── BusinessLogic/                # Business Logic Layer (BLL)
│   ├── Services/
│   │   ├── Interfaces/           # Service interfaces
│   │   └── Implements/           # Service implementations
│   └── DTOs/                     # Data Transfer Objects
│
├── DataAccess/                   # Data Access Layer (DAL)
│   ├── Entities/                 # EF Core Entities
│   ├── Repositories/
│   │   ├── Interfaces/           # Repository interfaces
│   │   └── Implements/           # Repository implementations
│   └── SchoolManagementDbContext.cs
│
└── PRN222_G5.sql                 # Database script
```

---

## ✨ Tính năng

| Tính năng                | Mô tả                                         |
| ------------------------ | --------------------------------------------- |
| 🔐 **Authentication**    | Đăng nhập/Đăng xuất với Cookie Authentication |
| 👤 **User Management**   | Quản lý Admin, Teacher, Student               |
| 📚 **Course Management** | CRUD khóa học                                 |
| 📝 **Enrollment**        | Đăng ký khóa học cho sinh viên                |
| 📊 **Grades**            | Quản lý điểm số                               |
| 💬 **Real-time Chat**    | Chat với SignalR                              |
| 🤖 **AI Chat**           | Tích hợp Gemini AI                            |
| 📧 **Forgot Password**   | Reset password qua email                      |

---

## 🔑 Tài khoản mặc định

| Role    | Username | Password |
| ------- | -------- | -------- |
| Admin   | admin    | admin123 |
| Teacher | teacher1 | 123456   |
| Student | student1 | 123456   |

> **Lưu ý:** Kiểm tra file `PRN222_G5.sql` để xem danh sách đầy đủ tài khoản seed.

---

## 🛠️ Troubleshooting

### Lỗi kết nối Database

```
Cannot open database "SchoolManagement" requested by the login
```

**Giải pháp:**

1. Đảm bảo SQL Server đang chạy
2. Kiểm tra connection string trong `appsettings.json`
3. Chạy lại file `PRN222_G5.sql`

### Lỗi Certificate

```
A connection was successfully established with the server, but then an error occurred during the login process
```

**Giải pháp:** Thêm `TrustServerCertificate=True` vào connection string

### Port đã được sử dụng

```bash
# Thay đổi port trong launchSettings.json hoặc chạy với port khác
dotnet run --urls "https://localhost:5002"
```

---

## 📝 License

This project is for educational purposes - PRN222 Course @ FPT University.
