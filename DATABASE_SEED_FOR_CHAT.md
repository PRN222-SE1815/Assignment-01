# 📊 DATABASE SEED DATA FOR CHAT FEATURE

> **Script SQL để tạo 10 records cho mỗi bảng**  
> **Mục đích:** Test tính năng chat real-time với đầy đủ dữ liệu

---

## 🔐 PASSWORD HASH

**Password:** `123456`  
**Hash:** `$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK`

---

## 📝 SQL SCRIPT

```sql
USE SchoolManagement;
GO

-- =============================================
-- CLEAR EXISTING DATA (Optional - Uncomment nếu muốn reset)
-- =============================================
/*
DELETE FROM NotificationRecipients;
DELETE FROM Notifications;
DELETE FROM MessageReads;
DELETE FROM Messages;
DELETE FROM ConversationParticipants;
DELETE FROM Conversations;
DELETE FROM Grades;
DELETE FROM Enrollments;
DELETE FROM CourseSchedules;
DELETE FROM Courses;
DELETE FROM Students;
DELETE FROM Teachers;
DELETE FROM Users WHERE RoleId != 1; -- Giữ admin
*/

-- =============================================
-- 1. INSERT USERS (10 Teachers + 10 Students + 1 Admin = 21 users)
-- =============================================

-- Admin (đã có sẵn)
-- Username: admin, Password: 123456

-- 10 TEACHERS
INSERT INTO Users (Username, PasswordHash, FullName, Email, IsActive, RoleId) VALUES
(N'teacher01', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Nguyễn Văn Hùng', N'hung.nguyen@school.edu.vn', 1, 2),
(N'teacher02', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Trần Thị Lan', N'lan.tran@school.edu.vn', 1, 2),
(N'teacher03', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Lê Minh Tuấn', N'tuan.le@school.edu.vn', 1, 2),
(N'teacher04', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Phạm Thu Hà', N'ha.pham@school.edu.vn', 1, 2),
(N'teacher05', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Hoàng Đức Anh', N'anh.hoang@school.edu.vn', 1, 2),
(N'teacher06', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Vũ Thị Mai', N'mai.vu@school.edu.vn', 1, 2),
(N'teacher07', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Đỗ Văn Nam', N'nam.do@school.edu.vn', 1, 2),
(N'teacher08', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Bùi Thị Ngọc', N'ngoc.bui@school.edu.vn', 1, 2),
(N'teacher09', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Cao Minh Khoa', N'khoa.cao@school.edu.vn', 1, 2),
(N'teacher10', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Đinh Thị Hương', N'huong.dinh@school.edu.vn', 1, 2);

-- 10 STUDENTS
INSERT INTO Users (Username, PasswordHash, FullName, Email, IsActive, RoleId) VALUES
(N'student01', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Lê Văn Nam', N'nam.le@student.edu.vn', 1, 3),
(N'student02', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Phạm Thị Hoa', N'hoa.pham@student.edu.vn', 1, 3),
(N'student03', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Hoàng Minh Tuấn', N'tuan.hoang@student.edu.vn', 1, 3),
(N'student04', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Nguyễn Thị Lan', N'lan.nguyen@student.edu.vn', 1, 3),
(N'student05', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Trần Văn Bình', N'binh.tran@student.edu.vn', 1, 3),
(N'student06', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Lý Thị Hương', N'huong.ly@student.edu.vn', 1, 3),
(N'student07', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Võ Minh Đức', N'duc.vo@student.edu.vn', 1, 3),
(N'student08', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Đặng Thị Thu', N'thu.dang@student.edu.vn', 1, 3),
(N'student09', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Phan Văn Long', N'long.phan@student.edu.vn', 1, 3),
(N'student10', N'$2a$11$hjqwTqzhwfClQUwEgfLfw.G0y378U4wW5Rocs6Htm4HPJfJunYpYK', N'Mai Thị Ngân', N'ngan.mai@student.edu.vn', 1, 3);
GO

-- =============================================
-- 2. INSERT TEACHERS (10 teachers)
-- =============================================
INSERT INTO Teachers (UserId, Department) VALUES
(2, N'Công nghệ thông tin'),
(3, N'Ngoại ngữ'),
(4, N'Toán học'),
(5, N'Vật lý'),
(6, N'Hóa học'),
(7, N'Sinh học'),
(8, N'Lịch sử'),
(9, N'Địa lý'),
(10, N'Kinh tế'),
(11, N'Kỹ thuật');
GO

-- =============================================
-- 3. INSERT STUDENTS (10 students)
-- =============================================
INSERT INTO Students (UserId, DateOfBirth, Major, EnrollmentYear) VALUES
(12, '2003-05-15', N'Kỹ thuật phần mềm', 2021),
(13, '2004-08-20', N'Ngôn ngữ Anh', 2022),
(14, '2003-12-10', N'Hệ thống thông tin', 2021),
(15, '2004-03-25', N'Khoa học máy tính', 2022),
(16, '2003-07-18', N'Mạng máy tính', 2021),
(17, '2004-11-05', N'An toàn thông tin', 2022),
(18, '2003-09-30', N'Trí tuệ nhân tạo', 2021),
(19, '2004-02-14', N'Khoa học dữ liệu', 2022),
(20, '2003-06-22', N'Công nghệ phần mềm', 2021),
(21, '2004-10-08', N'Điện toán đám mây', 2022);
GO

-- =============================================
-- 4. INSERT COURSES (10 courses)
-- =============================================
INSERT INTO Courses (CourseCode, CourseName, Credits, Semester, TeacherId) VALUES
(N'IT001', N'Nhập môn lập trình', 3, N'Fall 2024', 1),
(N'IT002', N'Cấu trúc dữ liệu', 4, N'Fall 2024', 1),
(N'IT003', N'Lập trình hướng đối tượng', 4, N'Fall 2024', 1),
(N'IT004', N'Cơ sở dữ liệu', 3, N'Fall 2024', 1),
(N'IT005', N'Mạng máy tính', 3, N'Fall 2024', 6),
(N'ENG101', N'Tiếng Anh cơ bản', 3, N'Fall 2024', 2),
(N'ENG102', N'Tiếng Anh nâng cao', 3, N'Fall 2024', 2),
(N'MATH101', N'Toán rời rạc', 3, N'Fall 2024', 3),
(N'MATH102', N'Giải tích', 4, N'Fall 2024', 3),
(N'PHY101', N'Vật lý đại cương', 3, N'Fall 2024', 4);
GO

-- =============================================
-- 5. INSERT COURSE SCHEDULES (10 schedules)
-- =============================================
INSERT INTO CourseSchedules (CourseId, DayOfWeek, StartTime, EndTime, StartDate, EndDate, Location, Note) VALUES
-- IT001: Mon & Wed 08:00-10:00
(1, 1, '08:00', '10:00', '2026-01-05', '2026-03-29', N'Room A101', N'IT001 - Lecture'),
(1, 3, '08:00', '10:00', '2026-01-05', '2026-03-29', N'Room A101', N'IT001 - Lecture'),

-- IT002: Tue & Thu 13:30-15:30
(2, 2, '13:30', '15:30', '2026-01-05', '2026-03-29', N'Room B203', N'IT002 - Lecture'),
(2, 4, '13:30', '15:30', '2026-01-05', '2026-03-29', N'Room B203', N'IT002 - Lecture'),

-- IT003: Fri 14:00-17:00
(3, 5, '14:00', '17:00', '2026-01-05', '2026-03-29', N'Lab C301', N'IT003 - Lab'),

-- IT004: Mon & Thu 10:00-12:00
(4, 1, '10:00', '12:00', '2026-01-05', '2026-03-29', N'Room A102', N'IT004 - Lecture'),
(4, 4, '10:00', '12:00', '2026-01-05', '2026-03-29', N'Lab C302', N'IT004 - Lab'),

-- IT005: Wed 13:00-16:00
(5, 3, '13:00', '16:00', '2026-01-05', '2026-03-29', N'Lab D201', N'IT005 - Lab'),

-- ENG101: Fri 09:00-11:00
(6, 5, '09:00', '11:00', '2026-01-05', '2026-03-29', N'Room C105', N'ENG101 - Lecture'),

-- MATH101: Tue 08:00-10:00
(8, 2, '08:00', '10:00', '2026-01-05', '2026-03-29', N'Room E201', N'MATH101 - Lecture');
GO

-- =============================================
-- 6. INSERT ENROLLMENTS (10 students x 3 courses = 30 enrollments)
-- Mỗi student enroll 3 môn để tạo nhiều conversation
-- =============================================
INSERT INTO Enrollments (StudentId, CourseId, EnrollDate, Status) VALUES
-- Student 1 (Nam): IT001, IT002, IT003
(1, 1, '2024-01-15', N'Active'),
(1, 2, '2024-01-15', N'Active'),
(1, 3, '2024-01-15', N'Active'),

-- Student 2 (Hoa): ENG101, IT001, MATH101
(2, 6, '2024-01-15', N'Active'),
(2, 1, '2024-01-15', N'Active'),
(2, 8, '2024-01-15', N'Active'),

-- Student 3 (Tuấn): IT001, IT002, IT004
(3, 1, '2024-01-15', N'Active'),
(3, 2, '2024-01-15', N'Active'),
(3, 4, '2024-01-15', N'Active'),

-- Student 4 (Lan): IT003, IT004, IT005
(4, 3, '2024-01-15', N'Active'),
(4, 4, '2024-01-15', N'Active'),
(4, 5, '2024-01-15', N'Active'),

-- Student 5 (Bình): IT001, IT003, ENG101
(5, 1, '2024-01-15', N'Active'),
(5, 3, '2024-01-15', N'Active'),
(5, 6, '2024-01-15', N'Active'),

-- Student 6 (Hương): IT002, IT004, MATH101
(6, 2, '2024-01-15', N'Active'),
(6, 4, '2024-01-15', N'Active'),
(6, 8, '2024-01-15', N'Active'),

-- Student 7 (Đức): IT001, IT005, ENG101
(7, 1, '2024-01-15', N'Active'),
(7, 5, '2024-01-15', N'Active'),
(7, 6, '2024-01-15', N'Active'),

-- Student 8 (Thu): IT002, IT003, IT004
(8, 2, '2024-01-15', N'Active'),
(8, 3, '2024-01-15', N'Active'),
(8, 4, '2024-01-15', N'Active'),

-- Student 9 (Long): IT001, IT002, MATH101
(9, 1, '2024-01-15', N'Active'),
(9, 2, '2024-01-15', N'Active'),
(9, 8, '2024-01-15', N'Active'),

-- Student 10 (Ngân): IT003, ENG101, MATH101
(10, 3, '2024-01-15', N'Active'),
(10, 6, '2024-01-15', N'Active'),
(10, 8, '2024-01-15', N'Active');
GO

-- =============================================
-- 7. INSERT GRADES (30 grades cho 30 enrollments)
-- =============================================
INSERT INTO Grades (EnrollmentId, Assignment, Midterm, Final, Total) VALUES
-- Student 1
(1, 8.5, 7.0, 8.0, 7.9),
(2, 9.0, 8.5, 9.0, 8.9),
(3, 7.5, 8.0, 7.5, 7.7),

-- Student 2
(4, 8.0, 7.5, 8.5, 8.1),
(5, 9.5, 9.0, 9.5, 9.4),
(6, 7.0, 7.5, 7.0, 7.2),

-- Student 3
(7, 6.0, 5.5, 6.0, 5.9),
(8, 7.5, 7.0, 7.5, 7.4),
(9, 8.5, 8.0, 8.5, 8.4),

-- Student 4
(10, 9.0, 8.5, 9.0, 8.9),
(11, 8.0, 7.5, 8.0, 7.9),
(12, 7.5, 7.0, 7.5, 7.4),

-- Student 5
(13, 8.5, 8.0, 8.5, 8.4),
(14, 9.0, 9.5, 9.0, 9.2),
(15, 7.0, 6.5, 7.0, 6.9),

-- Student 6
(16, 8.0, 8.5, 8.0, 8.2),
(17, 7.5, 7.0, 7.5, 7.4),
(18, 9.5, 9.0, 9.5, 9.4),

-- Student 7
(19, 7.0, 7.5, 7.0, 7.2),
(20, 8.5, 8.0, 8.5, 8.4),
(21, 9.0, 8.5, 9.0, 8.9),

-- Student 8
(22, 8.0, 7.5, 8.0, 7.9),
(23, 7.5, 8.0, 7.5, 7.7),
(24, 9.0, 9.5, 9.0, 9.2),

-- Student 9
(25, 8.5, 8.0, 8.5, 8.4),
(26, 7.0, 7.5, 7.0, 7.2),
(27, 9.5, 9.0, 9.5, 9.4),

-- Student 10
(28, 8.0, 8.5, 8.0, 8.2),
(29, 7.5, 7.0, 7.5, 7.4),
(30, 9.0, 8.5, 9.0, 8.9);
GO

-- =============================================
-- 8. INSERT CONVERSATIONS (10 course conversations)
-- ⭐ TỰ ĐỘNG TẠO KHI STUDENT ENROLL (qua EnrollmentService)
-- Nhưng để test, ta tạo thủ công trước
-- =============================================
INSERT INTO Conversations (IsGroup, Title, CreatedByUserId, CreatedAt, CourseId) VALUES
(1, N'IT001 - Nhập môn lập trình', 2, '2024-01-15 08:00:00', 1),
(1, N'IT002 - Cấu trúc dữ liệu', 2, '2024-01-15 08:00:00', 2),
(1, N'IT003 - Lập trình hướng đối tượng', 2, '2024-01-15 08:00:00', 3),
(1, N'IT004 - Cơ sở dữ liệu', 2, '2024-01-15 08:00:00', 4),
(1, N'IT005 - Mạng máy tính', 7, '2024-01-15 08:00:00', 5),
(1, N'ENG101 - Tiếng Anh cơ bản', 3, '2024-01-15 08:00:00', 6),
(1, N'ENG102 - Tiếng Anh nâng cao', 3, '2024-01-15 08:00:00', 7),
(1, N'MATH101 - Toán rời rạc', 4, '2024-01-15 08:00:00', 8),
(1, N'MATH102 - Giải tích', 4, '2024-01-15 08:00:00', 9),
(1, N'PHY101 - Vật lý đại cương', 5, '2024-01-15 08:00:00', 10);
GO

-- =============================================
-- 9. INSERT CONVERSATION PARTICIPANTS
-- Thêm Teacher + Students vào mỗi conversation
-- =============================================

-- IT001 Conversation (ConversationId = 1)
-- Teacher + Students: 1,2,3,5,7,9
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(1, 2, '2024-01-15 08:00:00'),  -- Teacher Hùng
(1, 12, '2024-01-15 08:05:00'), -- Student 1
(1, 13, '2024-01-15 08:06:00'), -- Student 2
(1, 14, '2024-01-15 08:07:00'), -- Student 3
(1, 16, '2024-01-15 08:08:00'), -- Student 5
(1, 18, '2024-01-15 08:09:00'), -- Student 7
(1, 20, '2024-01-15 08:10:00'); -- Student 9

-- IT002 Conversation (ConversationId = 2)
-- Teacher + Students: 1,3,6,8,9
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(2, 2, '2024-01-15 08:00:00'),  -- Teacher Hùng
(2, 12, '2024-01-15 08:05:00'), -- Student 1
(2, 14, '2024-01-15 08:06:00'), -- Student 3
(2, 17, '2024-01-15 08:07:00'), -- Student 6
(2, 19, '2024-01-15 08:08:00'), -- Student 8
(2, 20, '2024-01-15 08:09:00'); -- Student 9

-- IT003 Conversation (ConversationId = 3)
-- Teacher + Students: 1,4,5,8,10
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(3, 2, '2024-01-15 08:00:00'),  -- Teacher Hùng
(3, 12, '2024-01-15 08:05:00'), -- Student 1
(3, 15, '2024-01-15 08:06:00'), -- Student 4
(3, 16, '2024-01-15 08:07:00'), -- Student 5
(3, 19, '2024-01-15 08:08:00'), -- Student 8
(3, 21, '2024-01-15 08:09:00'); -- Student 10

-- IT004 Conversation (ConversationId = 4)
-- Teacher + Students: 3,4,6,8
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(4, 2, '2024-01-15 08:00:00'),  -- Teacher Hùng
(4, 14, '2024-01-15 08:05:00'), -- Student 3
(4, 15, '2024-01-15 08:06:00'), -- Student 4
(4, 17, '2024-01-15 08:07:00'), -- Student 6
(4, 19, '2024-01-15 08:08:00'); -- Student 8

-- IT005 Conversation (ConversationId = 5)
-- Teacher + Students: 4,7
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(5, 7, '2024-01-15 08:00:00'),  -- Teacher Nam
(5, 15, '2024-01-15 08:05:00'), -- Student 4
(5, 18, '2024-01-15 08:06:00'); -- Student 7

-- ENG101 Conversation (ConversationId = 6)
-- Teacher + Students: 2,5,7,10
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(6, 3, '2024-01-15 08:00:00'),  -- Teacher Lan
(6, 13, '2024-01-15 08:05:00'), -- Student 2
(6, 16, '2024-01-15 08:06:00'), -- Student 5
(6, 18, '2024-01-15 08:07:00'), -- Student 7
(6, 21, '2024-01-15 08:08:00'); -- Student 10

-- MATH101 Conversation (ConversationId = 8)
-- Teacher + Students: 2,6,9,10
INSERT INTO ConversationParticipants (ConversationId, UserId, JoinedAt) VALUES
(8, 4, '2024-01-15 08:00:00'),  -- Teacher Tuấn
(8, 13, '2024-01-15 08:05:00'), -- Student 2
(8, 17, '2024-01-15 08:06:00'), -- Student 6
(8, 20, '2024-01-15 08:07:00'), -- Student 9
(8, 21, '2024-01-15 08:08:00'); -- Student 10
GO

-- =============================================
-- 10. INSERT MESSAGES (10 messages per conversation)
-- Tạo 10 tin nhắn cho mỗi conversation để test chat
-- =============================================

-- IT001 Conversation Messages
INSERT INTO Messages (ConversationId, SenderUserId, Body, SentAt) VALUES
(1, 2, N'Chào mừng các bạn đến với môn Nhập môn lập trình!', '2024-01-15 08:15:00'),
(1, 12, N'Chào thầy ạ!', '2024-01-15 08:16:00'),
(1, 13, N'Em chào thầy!', '2024-01-15 08:17:00'),
(1, 2, N'Tuần này chúng ta sẽ học về biến và kiểu dữ liệu.', '2024-01-15 08:18:00'),
(1, 14, N'Thầy cho em hỏi bài tập về nhà ạ?', '2024-01-15 08:20:00'),
(1, 2, N'Các em làm bài tập từ 1 đến 5 trong sách giáo khoa nhé.', '2024-01-15 08:22:00'),
(1, 16, N'Dạ em cảm ơn thầy!', '2024-01-15 08:23:00'),
(1, 18, N'Thầy ơi, deadline nộp bài là khi nào ạ?', '2024-01-15 08:25:00'),
(1, 2, N'Deadline là thứ 6 tuần sau nhé các em.', '2024-01-15 08:26:00'),
(1, 20, N'Em đã hiểu rồi ạ. Cảm ơn thầy!', '2024-01-15 08:27:00');

-- IT002 Conversation Messages
INSERT INTO Messages (ConversationId, SenderUserId, Body, SentAt) VALUES
(2, 2, N'Hôm nay chúng ta học về Stack và Queue.', '2024-01-16 13:30:00'),
(2, 12, N'Thầy ơi, cho em hỏi sự khác nhau giữa Stack và Queue ạ?', '2024-01-16 13:35:00'),
(2, 2, N'Stack là LIFO (Last In First Out), Queue là FIFO (First In First Out).', '2024-01-16 13:37:00'),
(2, 14, N'Em hiểu rồi ạ, cảm ơn thầy!', '2024-01-16 13:38:00'),
(2, 17, N'Thầy cho em hỏi ứng dụng thực tế của Stack ạ?', '2024-01-16 13:40:00'),
(2, 2, N'Stack được dùng trong undo/redo, call stack của hàm, v.v.', '2024-01-16 13:42:00'),
(2, 19, N'Còn Queue thì sao thầy?', '2024-01-16 13:43:00'),
(2, 2, N'Queue dùng trong xử lý hàng đợi, BFS algorithm, task scheduling.', '2024-01-16 13:45:00'),
(2, 20, N'Em cảm ơn thầy nhiều ạ!', '2024-01-16 13:46:00'),
(2, 2, N'Các em về làm bài tập lab nhé!', '2024-01-16 13:47:00');

-- IT003 Conversation Messages
INSERT INTO Messages (ConversationId, SenderUserId, Body, SentAt) VALUES
(3, 2, N'Tuần này chúng ta học về Inheritance và Polymorphism.', '2024-01-17 14:00:00'),
(3, 12, N'Thầy ơi, cho em hỏi khi nào nên dùng Inheritance ạ?', '2024-01-17 14:05:00'),
(3, 2, N'Khi có mối quan hệ "is-a" giữa các class, ví dụ Dog is-a Animal.', '2024-01-17 14:07:00'),
(3, 15, N'Vậy Polymorphism là gì thầy?', '2024-01-17 14:08:00'),
(3, 2, N'Polymorphism là khả năng một object có thể có nhiều hình thái.', '2024-01-17 14:10:00'),
(3, 16, N'Thầy có thể cho ví dụ cụ thể không ạ?', '2024-01-17 14:11:00'),
(3, 2, N'Ví dụ: Animal animal = new Dog(); animal.makeSound() sẽ gọi Dog.makeSound().', '2024-01-17 14:13:00'),
(3, 19, N'Em hiểu rồi ạ!', '2024-01-17 14:14:00'),
(3, 21, N'Thầy cho em xin slide bài giảng với ạ.', '2024-01-17 14:15:00'),
(3, 2, N'Thầy đã upload lên LMS rồi nhé!', '2024-01-17 14:16:00');

-- ENG101 Conversation Messages
INSERT INTO Messages (ConversationId, SenderUserId, Body, SentAt) VALUES
(6, 3, N'Good morning everyone! Today we will learn about Present Perfect tense.', '2024-01-19 09:00:00'),
(6, 13, N'Good morning teacher!', '2024-01-19 09:01:00'),
(6, 16, N'Teacher, can you give us some examples?', '2024-01-19 09:03:00'),
(6, 3, N'Sure! For example: I have lived here for 5 years.', '2024-01-19 09:05:00'),
(6, 18, N'How is it different from Past Simple?', '2024-01-19 09:06:00'),
(6, 3, N'Past Simple is for completed actions, Present Perfect connects past to present.', '2024-01-19 09:08:00'),
(6, 21, N'I understand now, thank you!', '2024-01-19 09:09:00'),
(6, 13, N'Teacher, what is our homework?', '2024-01-19 09:10:00'),
(6, 3, N'Do exercises 1-10 on page 45.', '2024-01-19 09:11:00'),
(6, 16, N'Thank you teacher!', '2024-01-19 09:12:00');

-- MATH101 Conversation Messages
INSERT INTO Messages (ConversationId, SenderUserId, Body, SentAt) VALUES
(8, 4, N'Hôm nay chúng ta học về Graph Theory.', '2024-01-18 08:00:00'),
(8, 13, N'Thầy ơi, Graph là gì ạ?', '2024-01-18 08:05:00'),
(8, 4, N'Graph là tập hợp các đỉnh (vertices) và cạnh (edges) nối các đỉnh.', '2024-01-18 08:07:00'),
(8, 17, N'Cho em hỏi ứng dụng của Graph Theory ạ?', '2024-01-18 08:08:00'),
(8, 4, N'Ứng dụng rất nhiều: mạng xã hội, GPS, network routing, v.v.', '2024-01-18 08:10:00'),
(8, 20, N'Thầy có thể giải thích thuật toán Dijkstra không ạ?', '2024-01-18 08:11:00'),
(8, 4, N'Dijkstra dùng để tìm đường đi ngắn nhất từ 1 đỉnh đến các đỉnh khác.', '2024-01-18 08:13:00'),
(8, 21, N'Em cảm ơn thầy!', '2024-01-18 08:14:00'),
(8, 13, N'Thầy cho em xin tài liệu tham khảo với ạ.', '2024-01-18 08:15:00'),
(8, 4, N'Thầy sẽ gửi link vào group nhé!', '2024-01-18 08:16:00');
GO

-- =============================================
-- 11. INSERT MESSAGE READS (Mark some messages as read)
-- =============================================
-- IT001 Messages read by students
INSERT INTO MessageReads (MessageId, UserId, ReadAt) VALUES
(1, 12, '2024-01-15 08:16:00'),
(1, 13, '2024-01-15 08:17:00'),
(2, 2, '2024-01-15 08:17:00'),
(3, 2, '2024-01-15 08:18:00'),
(4, 12, '2024-01-15 08:19:00'),
(4, 13, '2024-01-15 08:19:00'),
(5, 2, '2024-01-15 08:21:00'),
(6, 14, '2024-01-15 08:23:00'),
(7, 2, '2024-01-15 08:24:00'),
(8, 2, '2024-01-15 08:26:00');
GO

-- =============================================
-- 12. INSERT NOTIFICATIONS (10 notifications)
-- =============================================
INSERT INTO Notifications (SenderUserId, Title, Message, CreatedAt) VALUES
(2, N'Bài tập tuần 1', N'Các em nhớ nộp bài tập IT001 trước thứ 6 nhé!', '2024-01-15 08:30:00'),
(2, N'Thông báo lịch thi', N'Lịch thi giữa kỳ IT002 đã được cập nhật.', '2024-01-16 14:00:00'),
(3, N'English Test', N'Midterm test for ENG101 will be on Friday.', '2024-01-19 09:30:00'),
(4, N'Bài giảng mới', N'Thầy đã upload bài giảng MATH101 tuần 3.', '2024-01-18 08:30:00'),
(2, N'Lab Session', N'Lab IT003 sẽ diễn ra vào thứ 6 tuần này.', '2024-01-17 14:30:00'),
(2, N'Thay đổi lịch học', N'Lịch học IT004 thứ 2 chuyển sang phòng A103.', '2024-01-15 10:00:00'),
(7, N'Network Lab', N'Chuẩn bị laptop cho lab IT005 tuần sau.', '2024-01-15 11:00:00'),
(3, N'Vocabulary Quiz', N'Quiz về từ vựng ENG101 vào thứ 3.', '2024-01-19 10:00:00'),
(4, N'Assignment Reminder', N'Nộp bài tập MATH101 chương 2 trước thứ 5.', '2024-01-18 09:00:00'),
(2, N'Final Project', N'Đề tài final project IT003 đã được công bố.', '2024-01-17 15:00:00');
GO

-- =============================================
-- 13. INSERT NOTIFICATION RECIPIENTS
-- =============================================
-- Notification 1 → Students in IT001
INSERT INTO NotificationRecipients (NotificationId, ReceiverUserId, IsRead, ReadAt) VALUES
(1, 12, 1, '2024-01-15 08:35:00'),
(1, 13, 1, '2024-01-15 08:40:00'),
(1, 14, 0, NULL),
(1, 16, 1, '2024-01-15 09:00:00'),
(1, 18, 0, NULL),
(1, 20, 1, '2024-01-15 08:50:00');

-- Notification 2 → Students in IT002
INSERT INTO NotificationRecipients (NotificationId, ReceiverUserId, IsRead, ReadAt) VALUES
(2, 12, 1, '2024-01-16 14:10:00'),
(2, 14, 1, '2024-01-16 14:15:00'),
(2, 17, 0, NULL),
(2, 19, 1, '2024-01-16 14:20:00'),
(2, 20, 0, NULL);

-- Notification 3 → Students in ENG101
INSERT INTO NotificationRecipients (NotificationId, ReceiverUserId, IsRead, ReadAt) VALUES
(3, 13, 1, '2024-01-19 09:35:00'),
(3, 16, 1, '2024-01-19 09:40:00'),
(3, 18, 0, NULL),
(3, 21, 1, '2024-01-19 09:45:00');
GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL
SELECT 'Teachers', COUNT(*) FROM Teachers
UNION ALL
SELECT 'Students', COUNT(*) FROM Students
UNION ALL
SELECT 'Courses', COUNT(*) FROM Courses
UNION ALL
SELECT 'CourseSchedules', COUNT(*) FROM CourseSchedules
UNION ALL
SELECT 'Enrollments', COUNT(*) FROM Enrollments
UNION ALL
SELECT 'Grades', COUNT(*) FROM Grades
UNION ALL
SELECT 'Conversations', COUNT(*) FROM Conversations
UNION ALL
SELECT 'ConversationParticipants', COUNT(*) FROM ConversationParticipants
UNION ALL
SELECT 'Messages', COUNT(*) FROM Messages
UNION ALL
SELECT 'MessageReads', COUNT(*) FROM MessageReads
UNION ALL
SELECT 'Notifications', COUNT(*) FROM Notifications
UNION ALL
SELECT 'NotificationRecipients', COUNT(*) FROM NotificationRecipients;

-- Check conversations with participants
SELECT 
    c.ConversationId,
    c.Title,
    COUNT(cp.UserId) AS ParticipantCount
FROM Conversations c
LEFT JOIN ConversationParticipants cp ON c.ConversationId = cp.ConversationId
WHERE cp.LeftAt IS NULL
GROUP BY c.ConversationId, c.Title
ORDER BY c.ConversationId;

-- Check messages per conversation
SELECT 
    c.ConversationId,
    c.Title,
    COUNT(m.MessageId) AS MessageCount
FROM Conversations c
LEFT JOIN Messages m ON c.ConversationId = m.ConversationId
WHERE m.IsDeleted = 0
GROUP BY c.ConversationId, c.Title
ORDER BY c.ConversationId;
```

---

## 📊 SUMMARY

### **Record Counts:**
- **Users:** 21 (1 Admin + 10 Teachers + 10 Students)
- **Teachers:** 10
- **Students:** 10
- **Courses:** 10
- **CourseSchedules:** 10
- **Enrollments:** 30 (10 students x 3 courses)
- **Grades:** 30
- **Conversations:** 10 (1 per course)
- **ConversationParticipants:** ~50 (teacher + students per course)
- **Messages:** 50 (10 per conversation x 5 conversations)
- **MessageReads:** 10+
- **Notifications:** 10
- **NotificationRecipients:** 20+

---

## 🔐 LOGIN CREDENTIALS

### **Admin:**
- Username: `admin`
- Password: `123456`

### **Teachers:**
- Username: `teacher01` → `teacher10`
- Password: `123456`

### **Students:**
- Username: `student01` → `student10`
- Password: `123456`

---

## ✅ USAGE

1. **Chạy script** trong SQL Server Management Studio
2. **Verify data** bằng các query cuối file
3. **Test login** với các accounts trên
4. **Test chat** giữa students và teachers trong course conversations

---

**Happy Testing! 💬🚀**

