# 🏫 School Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet)

Hệ thống quản lý trường học được xây dựng với **ASP.NET Core MVC** theo kiến trúc 3 lớp (3-tier Architecture)

[Tính năng](#-tính-năng) • [Cài đặt](#-cài-đặt) • [Cấu hình](#️-cấu-hình) • [Sử dụng](#-sử-dụng)

</div>

---

## 📋 Mục lục

- [Giới thiệu](#-giới-thiệu)
- [Tính năng](#-tính-năng)
- [Kiến trúc](#-kiến-trúc)
- [Yêu cầu hệ thống](#-yêu-cầu-hệ-thống)
- [Cài đặt](#-cài-đặt)
- [Cấu hình](#️-cấu-hình)
- [Chạy ứng dụng](#-chạy-ứng-dụng)
- [Cấu trúc dự án](#-cấu-trúc-dự-án)
- [Đóng góp](#-đóng-góp)

---

## 📖 Giới thiệu

**School Management System** là một ứng dụng web quản lý trường học toàn diện được phát triển cho môn học **PRN222** tại **FPT University SE1815**. Hệ thống cung cấp các chức năng quản lý học sinh, giáo viên, lớp học, môn học và điểm số một cách hiệu quả.

### 🎯 Mục tiêu dự án

- Áp dụng kiến trúc 3 lớp (3-tier Architecture) trong ASP.NET Core MVC
- Quản lý dữ liệu với Entity Framework Core
- Xây dựng giao diện người dùng thân thiện và responsive
- Thực hiện các thao tác CRUD cơ bản và nâng cao
- Áp dụng các best practices trong phát triển web

---

## ✨ Tính năng

### 👨‍🎓 Quản lý học sinh
- ➕ Thêm, sửa, xóa thông tin học sinh
- 🔍 Tìm kiếm và lọc học sinh
- 📊 Xem thông tin chi tiết và lịch sử học tập
- 📈 Quản lý điểm số và kết quả học tập

### 👨‍🏫 Quản lý giáo viên
- ➕ Quản lý thông tin giáo viên
- 📚 Phân công giảng dạy môn học
- 📅 Quản lý lịch dạy

### 📚 Quản lý lớp học & môn học
- 🏛️ Tạo và quản lý lớp học
- 📖 Quản lý danh sách môn học
- 👥 Phân công học sinh vào lớp
- 🔗 Gán giáo viên cho môn học

### 🔐 Xác thực & Phân quyền
- 🔑 Đăng nhập/Đăng xuất
- 👤 Quản lý tài khoản người dùng
- 🛡️ Phân quyền theo vai trò (Admin, Teacher, Student)
- 🔒 Quên mật khẩu (SMTP Email)

### 📊 Báo cáo & Thống kê
- 📈 Báo cáo kết quả học tập
- 📉 Thống kê theo lớp, môn học
- 📋 Xuất danh sách và báo cáo

---

## 🏗️ Kiến trúc

Dự án được xây dựng theo mô hình **3-tier Architecture**:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│              (Web - MVC)                │
│  - Controllers                          │
│  - Views (Razor)                        │
│  - ViewModels                           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Business Logic Layer            │
│          (BusinessLogic)                │
│  - Services                             │
│  - Business Rules                       │
│  - Validation                           │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Data Access Layer              │
│    (DataAccess + BusinessObject)        │
│  - Repositories                         │
│  - Entity Models                        │
│  - DbContext                            │
└─────────────────────────────────────────┘
               │
               ▼
         [SQL Server Database]
```

### 📦 Các Layer

| Layer | Thư mục | Mô tả |
|-------|---------|-------|
| **Presentation** | `Web/` | Xử lý giao diện người dùng, Controllers, Views, wwwroot |
| **Business Logic** | `BusinessLogic/` | Chứa logic nghiệp vụ, services, validation |
| **Data Access** | `DataAccess/` | Repositories, DbContext, truy xuất dữ liệu |
| **Business Objects** | `BusinessObject/` | Entity models, DTOs |

---

## 💻 Yêu cầu hệ thống

### Phần mềm bắt buộc

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) hoặc cao hơn
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (LocalDB, Express, hoặc Developer Edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) hoặc [JetBrains Rider](https://www.jetbrains.com/rider/)
- [Git](https://git-scm.com/)

### Công cụ hỗ trợ (tùy chọn)

- [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup)
- [Azure Data Studio](https://azure.microsoft.com/products/data-studio/)
- [Postman](https://www.postman.com/) (để test API nếu có)

---

## 🚀 Cài đặt

### 1️⃣ Clone repository

```bash
git clone https://github.com/PRN222-SE1815/Assignment-01.git
cd Assignment-01
```

### 2️⃣ Restore các packages

```bash
dotnet restore
```

### 3️⃣ Tạo Database

#### Cách 1: Sử dụng SQL Server Management Studio (SSMS)

1. Mở **SSMS** và kết nối đến SQL Server
2. Chạy file `PRN222_G5.sql` để tạo database và schema
3. (Tùy chọn) Chạy file `seed_PRN222_G5_v3.sql` để seed dữ liệu mẫu

#### Cách 2: Sử dụng Command Line

```bash
# Windows Authentication
sqlcmd -S . -E -i PRN222_G5.sql
sqlcmd -S . -E -i seed_PRN222_G5_v3.sql

# SQL Server Authentication
sqlcmd -S . -U sa -P YOUR_PASSWORD -i PRN222_G5.sql
sqlcmd -S . -U sa -P YOUR_PASSWORD -i seed_PRN222_G5_v3.sql
```

> **📝 Lưu ý:** Các file SQL sẽ tạo database `SchoolManagement` (hoặc `PRN222_G5`) với đầy đủ bảng và dữ liệu mẫu.

---

## ⚙️ Cấu hình

### 1. Connection String

Mở file `Web/appsettings.json` và cập nhật connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=SchoolManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### 🔧 Các loại Connection String

| Loại kết nối | Connection String |
|--------------|-------------------|
| **Windows Authentication** | `Server=.;Database=SchoolManagement;Trusted_Connection=True;TrustServerCertificate=True;` |
| **SQL Server Authentication** | `Server=.;Database=SchoolManagement;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;` |
| **LocalDB** | `Server=(localdb)\mssqllocaldb;Database=SchoolManagement;Trusted_Connection=True;` |
| **Remote Server** | `Server=YOUR_SERVER;Database=SchoolManagement;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;` |

> **⚠️ Lưu ý:** Với Gmail, bạn cần tạo [App Password](https://support.google.com/accounts/answer/185833) thay vì dùng mật khẩu thường.

### 3. Cấu hình môi trường (Environment)

```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development

# Production
export ASPNETCORE_ENVIRONMENT=Production
```

---

## 🎮 Chạy ứng dụng

### Cách 1: Sử dụng .NET CLI

```bash
cd Web
dotnet run
```

Ứng dụng sẽ chạy tại:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001

### Cách 2: Sử dụng Visual Studio

1. Mở file `Assignment01_SchoolManagement.slnx`
2. Đặt project **Web** làm Startup Project
3. Nhấn `F5` hoặc click **Run**

### Cách 3: Sử dụng Visual Studio Code

1. Mở thư mục project trong VS Code
2. Nhấn `F5` hoặc chạy task `.NET Core Launch (web)`

---

## 📁 Cấu trúc dự án

```
Assignment-01/
│
├── 📂 Web/                          # Presentation Layer
│   ├── Controllers/                 # MVC Controllers
│   ├── Views/                       # Razor Views
│   ├── wwwroot/                     # Static files (CSS, JS, images)
│   ├── Models/ViewModels/           # View Models
│   ├── appsettings.json             # Configuration
│   └── Program.cs                   # Entry point
│
├── 📂 BusinessLogic/                # Business Logic Layer
│   ├── Services/                    # Business services
│   ├── Interfaces/                  # Service interfaces
│   └── Validators/                  # Business validation
│
├── 📂 DataAccess/                   # Data Access Layer
│   ├── Repositories/                # Repository pattern
│   ├── DbContext/                   # EF Core DbContext
│   └── Interfaces/                  # Repository interfaces
│
├── 📂 BusinessObject/               # Domain Models
│   ├── Models/                      # Entity classes
│   └── DTOs/                        # Data Transfer Objects
│
├── 📄 PRN222_G5.sql                 # Database schema script
├── 📄 seed_PRN222_G5_v3.sql         # Seed data script
├── 📄 Assignment01_SchoolManagement.slnx  # Solution file
└── 📄 README.md                     # Documentation
```

---

## 🔑 Tài khoản mẫu

Sau khi seed data, bạn có thể sử dụng các tài khoản sau để đăng nhập:

| Vai trò | Username | Password | Mô tả |
|---------|----------|----------|-------|
| **Admin** | `admin` | `admin123` | Quản trị viên hệ thống |
| **Teacher** | `teacher1` | `teacher123` | Giáo viên |
| **Student** | `student1` | `student123` | Học sinh |

> **⚠️ Quan trọng:** Đổi mật khẩu ngay sau lần đăng nhập đầu tiên trong môi trường production!

---

## 🛠️ Công nghệ sử dụng

### Backend
- **Framework:** ASP.NET Core 8.0 MVC
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Authentication:** ASP.NET Core Identity
- **DI Container:** Built-in Dependency Injection

### Frontend
- **Template Engine:** Razor Pages
- **CSS Framework:** Bootstrap 5
- **JavaScript:** jQuery, Vanilla JS
- **Icons:** Font Awesome

### Kiến trúc & Patterns
- 3-Tier Architecture
- Repository Pattern
- Dependency Injection
- Service Layer Pattern
- Model-View-Controller (MVC)

---

## 📚 Tài liệu tham khảo

- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [SQL Server Documentation](https://learn.microsoft.com/sql/)
- [Bootstrap 5](https://getbootstrap.com/docs/5.0/)

---

## 🤝 Đóng góp

Dự án này được phát triển cho mục đích học tập tại FPT University. Nếu bạn muốn đóng góp:

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Mở Pull Request

---

## 👥 Nhóm phát triển

**PRN222 - SE1815 - Group 5**
- Nguyễn Minh Tấn - SE182211
- Võ Hải Hào - SE182170
- Phạm Đức Hùng - SE182153
- Lê Tiến Đạt - SE182453
- Hồng Lê Đăng Khoa - SE182425

---

## 📝 License

Dự án này được phát triển cho mục đích học tập tại FPT University.

---

## 📧 Liên hệ

Nếu có bất kỳ câu hỏi nào, vui lòng liên hệ qua:
- 🔗 GitHub Issues: [Issues](https://github.com/PRN222-SE1815/Assignment-01/issues)
- 📧 Email: datletien1352004@gmail.com

---

<div align="center">

**⭐ Nếu project này hữu ích, hãy cho chúng tôi một star! ⭐**

Made with ❤️ by PRN222-SE1815-Group5

</div>
