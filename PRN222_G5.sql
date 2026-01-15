-- Create Database
CREATE DATABASE SchoolManagement;
GO

USE SchoolManagement;
GO

-- Table: Roles
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

-- Table: Users
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    RoleId INT,
    FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Table: Teachers
CREATE TABLE Teachers (
    TeacherId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Department NVARCHAR(100),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Table: Students
CREATE TABLE Students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    DateOfBirth DATE,
    Major NVARCHAR(100),
    EnrollmentYear INT,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Table: Courses
CREATE TABLE Courses (
    CourseId INT IDENTITY(1,1) PRIMARY KEY,
    CourseCode NVARCHAR(20) NOT NULL UNIQUE,
    CourseName NVARCHAR(200) NOT NULL,
    Credits INT,
    Semester NVARCHAR(20),
    TeacherId INT,
    FOREIGN KEY (TeacherId) REFERENCES Teachers(TeacherId)
);

-- Table: Enrollments
CREATE TABLE Enrollments (
    EnrollmentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrollDate DATE DEFAULT CAST(SYSDATETIME() AS DATE),
    Status NVARCHAR(20) DEFAULT N'Active',
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

-- Table: Grades
CREATE TABLE Grades (
    GradeId INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentId INT NOT NULL,
    Assignment DECIMAL(5,2),
    Midterm DECIMAL(5,2),
    Final DECIMAL(5,2),
    Total DECIMAL(5,2),
    FOREIGN KEY (EnrollmentId) REFERENCES Enrollments(EnrollmentId)
);

-- Table: Conversations
CREATE TABLE Conversations (
    ConversationId INT IDENTITY(1,1) PRIMARY KEY,
    IsGroup BIT DEFAULT 0,
    Title NVARCHAR(200),
    CreatedByUserId INT,
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
);

-- Table: ConversationParticipants
CREATE TABLE ConversationParticipants (
    ConversationId INT,
    UserId INT,
    JoinedAt DATETIME2 DEFAULT SYSDATETIME(),
    LeftAt DATETIME2,
    PRIMARY KEY (ConversationId, UserId),
    FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Table: Messages
CREATE TABLE Messages (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderUserId INT NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 DEFAULT SYSDATETIME(),
    EditedAt DATETIME2,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId),
);

-- Table: MessageReads
CREATE TABLE MessageReads (
    MessageId INT,
    UserId INT,
    ReadAt DATETIME2 DEFAULT SYSDATETIME(),
    PRIMARY KEY (MessageId, UserId),
    FOREIGN KEY (MessageId) REFERENCES Messages(MessageId),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Table: Notifications
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    SenderUserId INT,
    Title NVARCHAR(200),
    Message NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    FOREIGN KEY (SenderUserId) REFERENCES Users(UserId)
);

-- Table: NotificationRecipients
CREATE TABLE NotificationRecipients (
    NotificationId INT,
    ReceiverUserId INT,
    IsRead BIT DEFAULT 0,
    ReadAt DATETIME2,
    PRIMARY KEY (NotificationId, ReceiverUserId),
    FOREIGN KEY (NotificationId) REFERENCES Notifications(NotificationId),
    FOREIGN KEY (ReceiverUserId) REFERENCES Users(UserId)
);

-- Insert sample data
INSERT INTO Roles (RoleName) VALUES 
(N'Admin'),
(N'Teacher'),
(N'Student');

-- 1. Insert Users (Bao gồm Admin, Giáo viên và Học sinh)
-- Lưu ý: PasswordHash ở đây chỉ là demo string, thực tế phải là chuỗi đã mã hóa
INSERT INTO Users (Username, PasswordHash, FullName, Email, IsActive, RoleId) VALUES 
(N'admin', N'hash_admin_123', N'Quản Trị Viên', N'admin@school.edu.vn', 1, 1), -- ID: 1 (Admin)
(N'thay_hung', N'hash_hung_123', N'Nguyễn Văn Hùng', N'hung.nguyen@school.edu.vn', 1, 2), -- ID: 2 (Teacher)
(N'co_lan', N'hash_lan_123', N'Trần Thị Lan', N'lan.tran@school.edu.vn', 1, 2), -- ID: 3 (Teacher)
(N'sv_nam', N'hash_nam_123', N'Lê Văn Nam', N'nam.le@student.edu.vn', 1, 3), -- ID: 4 (Student)
(N'sv_hoa', N'hash_hoa_123', N'Phạm Thị Hoa', N'hoa.pham@student.edu.vn', 1, 3), -- ID: 5 (Student)
(N'sv_tuan', N'hash_tuan_123', N'Hoàng Minh Tuấn', N'tuan.hoang@student.edu.vn', 1, 3); -- ID: 6 (Student)
GO

-- 2. Insert Teachers (Liên kết với User ID 2 và 3)
INSERT INTO Teachers (UserId, Department) VALUES 
(2, N'Công nghệ thông tin'), -- Thầy Hùng
(3, N'Ngoại ngữ'); -- Cô Lan
GO

-- 3. Insert Students (Liên kết với User ID 4, 5, 6)
INSERT INTO Students (UserId, DateOfBirth, Major, EnrollmentYear) VALUES 
(4, '2003-05-15', N'Kỹ thuật phần mềm', 2021),
(5, '2004-08-20', N'Ngôn ngữ Anh', 2022),
(6, '2003-12-10', N'Hệ thống thông tin', 2021);
GO

-- 4. Insert Courses (Các môn học do giáo viên phụ trách)
INSERT INTO Courses (CourseCode, CourseName, Credits, Semester, TeacherId) VALUES 
(N'IT001', N'Nhập môn lập trình', 3, N'Fall 2024', 1), -- Thầy Hùng dạy
(N'IT002', N'Cấu trúc dữ liệu', 4, N'Fall 2024', 1), -- Thầy Hùng dạy
(N'ENG101', N'Tiếng Anh cơ bản', 3, N'Fall 2024', 2); -- Cô Lan dạy
GO

-- 5. Insert Enrollments (Sinh viên đăng ký môn học)
INSERT INTO Enrollments (StudentId, CourseId, Status) VALUES 
(1, 1, N'Active'), -- Nam học IT001
(1, 2, N'Active'), -- Nam học IT002
(2, 3, N'Active'), -- Hoa học Tiếng Anh
(3, 1, N'Active'); -- Tuấn học IT001
GO

-- 6. Insert Grades (Điểm số cho các lượt đăng ký học)
-- Giả sử EnrollmentId 1, 2, 3, 4 tương ứng với lệnh insert ở trên
INSERT INTO Grades (EnrollmentId, Assignment, Midterm, Final, Total) VALUES 
(1, 8.5, 7.0, 8.0, 7.9), -- Điểm của Nam môn IT001
(2, 9.0, 8.5, 9.0, 8.9), -- Điểm của Nam môn IT002
(3, 7.5, 8.0, 7.5, 7.7), -- Điểm của Hoa môn Tiếng Anh
(4, 6.0, 5.5, 6.0, 5.9); -- Điểm của Tuấn môn IT001
GO
GO
