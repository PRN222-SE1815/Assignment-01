/* ============================================================
   SEED DATA for PRN222_G5.sql (SQL Server)
   - Insert sample data for ALL tables EXCEPT dbo.Users
   - Each table >= 5 records
   - Assumes dbo.Users already contains:
       - >= 5 users with Role='STUDENT'
       - >= 5 users with Role='TEACHER'
       - >= 1 user with Role='ADMIN'
   ============================================================ */
   BEGIN TRAN;

DELETE FROM dbo.AIToolCalls;
DELETE FROM dbo.AIChatMessages;
DELETE FROM dbo.AIChatSessions;

DELETE FROM dbo.NotificationRecipients;
DELETE FROM dbo.Notifications;

DELETE FROM dbo.ScheduleChangeLogs;
DELETE FROM dbo.ScheduleEventOverrides;
DELETE FROM dbo.ScheduleEvents;
DELETE FROM dbo.Recurrences;

DELETE FROM dbo.ChatMessageAttachments;
DELETE FROM dbo.ChatModerationLogs;
DELETE FROM dbo.ChatMessages;
DELETE FROM dbo.ChatRoomMembers;
DELETE FROM dbo.ChatRooms;

DELETE FROM dbo.GradeAuditLogs;
DELETE FROM dbo.GradeEntries;
DELETE FROM dbo.GradeItems;
DELETE FROM dbo.GradeBooks;

DELETE FROM dbo.Enrollments;
DELETE FROM dbo.ClassSections;
DELETE FROM dbo.CoursePrerequisites;

DELETE FROM dbo.Courses;
DELETE FROM dbo.Teachers;
DELETE FROM dbo.Students;

DELETE FROM dbo.Semesters;
DELETE FROM dbo.Programs;

COMMIT;


SET NOCOUNT ON;
GO

/* Use your DB name if different */
IF DB_ID(N'SchoolManagementDb') IS NOT NULL
BEGIN
    USE SchoolManagementDb;
END
GO

BEGIN TRY
BEGIN TRAN;

----------------------------------------------------------------
-- 0) Required Users (NOT inserting users, only reading IDs)
----------------------------------------------------------------
DECLARE @AdminUserId INT;

SELECT TOP (1) @AdminUserId = UserId
FROM dbo.Users
WHERE Role = N'ADMIN' AND IsActive = 1
ORDER BY UserId;

IF @AdminUserId IS NULL
    THROW 51000, 'Seed failed: Need at least 1 ADMIN user in dbo.Users (Role=ADMIN).', 1;

DECLARE @StudentUsers TABLE (RowNum INT IDENTITY(1,1), UserId INT);
INSERT INTO @StudentUsers(UserId)
SELECT TOP (10) UserId
FROM dbo.Users
WHERE Role = N'STUDENT' AND IsActive = 1
ORDER BY UserId;

IF (SELECT COUNT(*) FROM @StudentUsers) < 5
    THROW 51001, 'Seed failed: Need at least 5 STUDENT users in dbo.Users (Role=STUDENT).', 1;

DECLARE @TeacherUsers TABLE (RowNum INT IDENTITY(1,1), UserId INT);
INSERT INTO @TeacherUsers(UserId)
SELECT TOP (10) UserId
FROM dbo.Users
WHERE Role = N'TEACHER' AND IsActive = 1
ORDER BY UserId;

IF (SELECT COUNT(*) FROM @TeacherUsers) < 5
    THROW 51002, 'Seed failed: Need at least 5 TEACHER users in dbo.Users (Role=TEACHER).', 1;

----------------------------------------------------------------
-- 1) Programs (>=5)
----------------------------------------------------------------
-- Idempotent upsert for Programs (by ProgramCode)
MERGE dbo.Programs AS tgt
USING (VALUES
 (N'IT',  N'Information Technology', 1),
 (N'SE',  N'Software Engineering',   1),
 (N'AI',  N'Artificial Intelligence',1),
 (N'BA',  N'Business Analytics',     1),
 (N'GD',  N'Graphic Design',         1)
) AS src(ProgramCode, ProgramName, IsActive)
ON tgt.ProgramCode = src.ProgramCode
WHEN NOT MATCHED THEN
  INSERT(ProgramCode, ProgramName, IsActive)
  VALUES(src.ProgramCode, src.ProgramName, src.IsActive);

----------------------------------------------------------------
-- 2) Semesters (>=5)
----------------------------------------------------------------
-- Idempotent upsert for Semesters (by SemesterCode)
MERGE dbo.Semesters AS tgt
USING (VALUES
 (N'2024-FA', N'Fall 2024',   '2024-09-01','2024-12-20', 0, '2024-09-10','2024-09-25','2024-11-15', 16, 8),
 (N'2025-SP', N'Spring 2025', '2025-01-10','2025-05-10', 0, '2025-01-20','2025-02-05','2025-04-10', 18, 8),
 (N'2025-SU', N'Summer 2025', '2025-06-01','2025-08-15', 0, '2025-06-05','2025-06-15','2025-07-25', 10, 4),
 (N'2025-FA', N'Fall 2025',   '2025-09-01','2025-12-20', 0, '2025-09-10','2025-09-25','2025-11-15', 16, 8),
 (N'2026-SP', N'Spring 2026', '2026-01-15','2026-05-15', 1, '2026-02-05','2026-02-07','2026-05-15', 18, 8)
) AS src(
    SemesterCode, SemesterName, StartDate, EndDate, IsActive,
    RegistrationEndDate, AddDropDeadline, WithdrawalDeadline,
    MaxCredits, MinCredits
)
ON tgt.SemesterCode = src.SemesterCode
WHEN NOT MATCHED THEN
  INSERT(
      SemesterCode, SemesterName, StartDate, EndDate, IsActive,
      RegistrationEndDate, AddDropDeadline, WithdrawalDeadline,
      MaxCredits, MinCredits
  )
  VALUES(
      src.SemesterCode, src.SemesterName, src.StartDate, src.EndDate, src.IsActive,
      src.RegistrationEndDate, src.AddDropDeadline, src.WithdrawalDeadline,
      src.MaxCredits, src.MinCredits
  );

----------------------------------------------------------------
-- 3) Courses (>=5)
----------------------------------------------------------------
-- Idempotent upsert for Courses (by CourseCode)
MERGE dbo.Courses AS tgt
USING (VALUES
 (N'PRN221', N'.NET Web Fundamentals', 3, N'Intro to .NET web development', N'<p>Basics of ASP.NET</p>', N'{"weeks":[1,2,3,4]}', 1),
 (N'PRN222', N'.NET Web Advanced',     3, N'Advanced ASP.NET Core / EF Core', N'<p>Repository + Service</p>', N'{"weeks":[5,6,7,8]}', 1),
 (N'DBI202', N'Database Systems',      3, N'SQL design and optimization', N'<p>Normalization, Index</p>', N'{"topics":["SQL","Index","Tx"]}', 1),
 (N'WEB101', N'Web Fundamentals',      3, N'HTML/CSS/JS basics', N'<p>HTML, CSS, JS</p>', N'{"labs":[1,2,3]}', 1),
 (N'SWE102', N'Software Engineering',  3, N'Process, UML, testing', N'<p>UML, Agile</p>', N'{"sprints":[1,2]}', 1),
 (N'AI201',  N'Intro to Machine Learning', 3, N'Basics of ML', N'<p>Supervised learning</p>', N'{"units":["regression","classification"]}', 1),
 (N'MAD101', N'Mobile App Development',2, N'Android basics', N'<p>Activities, intents</p>', N'{"projects":["todo","chat"]}', 1)
) AS src(CourseCode, CourseName, Credits, Description, ContentHtml, LearningPathJson, IsActive)
ON tgt.CourseCode = src.CourseCode
WHEN NOT MATCHED THEN
  INSERT(CourseCode, CourseName, Credits, Description, ContentHtml, LearningPathJson, IsActive)
  VALUES(src.CourseCode, src.CourseName, src.Credits, src.Description, src.ContentHtml, src.LearningPathJson, src.IsActive);

----------------------------------------------------------------
-- 4) CoursePrerequisites (>=5)
----------------------------------------------------------------
INSERT INTO dbo.CoursePrerequisites(CourseId, PrerequisiteCourseId)
SELECT c.CourseId, p.CourseId
FROM (VALUES
  (N'PRN222', N'PRN221'),
  (N'SWE102', N'WEB101'),
  (N'AI201',  N'DBI202'),
  (N'PRN222', N'DBI202'),
  (N'MAD101', N'WEB101')
) v(CourseCode, PrereqCode)
JOIN dbo.Courses c ON c.CourseCode = v.CourseCode
JOIN dbo.Courses p ON p.CourseCode = v.PrereqCode
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.CoursePrerequisites cp
    WHERE cp.CourseId = c.CourseId
      AND cp.PrerequisiteCourseId = p.CourseId
);

----------------------------------------------------------------
-- 5) Students (>=5) (StudentId = Users.UserId)
----------------------------------------------------------------
DECLARE @ActiveSemesterId INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterCode = N'2026-SP');
DECLARE @ProgramIT INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = N'IT');
DECLARE @ProgramSE INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = N'SE');
DECLARE @ProgramAI INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = N'AI');
DECLARE @ProgramBA INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = N'BA');
DECLARE @ProgramGD INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = N'GD');

INSERT INTO dbo.Students(StudentId, StudentCode, ProgramId, CurrentSemesterId)
SELECT su.UserId,
       N'SV' + RIGHT(N'0000' + CAST(su.UserId AS NVARCHAR(10)), 4),
       CASE su.RowNum
            WHEN 1 THEN @ProgramIT
            WHEN 2 THEN @ProgramSE
            WHEN 3 THEN @ProgramAI
            WHEN 4 THEN @ProgramBA
            ELSE @ProgramGD
       END,
       @ActiveSemesterId
FROM (SELECT TOP (5) RowNum, UserId FROM @StudentUsers ORDER BY RowNum) su
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Students s WHERE s.StudentId = su.UserId
);

----------------------------------------------------------------
-- 6) Teachers (>=5) (TeacherId = Users.UserId)
----------------------------------------------------------------
INSERT INTO dbo.Teachers(TeacherId, TeacherCode, Department)
SELECT tu.UserId,
       N'T' + RIGHT(N'0000' + CAST(tu.UserId AS NVARCHAR(10)), 4),
       CASE tu.RowNum
            WHEN 1 THEN N'Computer Science'
            WHEN 2 THEN N'Information Systems'
            WHEN 3 THEN N'AI & Data'
            WHEN 4 THEN N'Software Engineering'
            ELSE N'General Education'
       END
FROM (SELECT TOP (5) RowNum, UserId FROM @TeacherUsers ORDER BY RowNum) tu
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Teachers t WHERE t.TeacherId = tu.UserId
);

----------------------------------------------------------------
-- 7) ClassSections (>=5)
----------------------------------------------------------------
DECLARE @Teacher1 INT = (SELECT UserId FROM @TeacherUsers WHERE RowNum = 1);
DECLARE @Teacher2 INT = (SELECT UserId FROM @TeacherUsers WHERE RowNum = 2);
DECLARE @Teacher3 INT = (SELECT UserId FROM @TeacherUsers WHERE RowNum = 3);
DECLARE @Teacher4 INT = (SELECT UserId FROM @TeacherUsers WHERE RowNum = 4);
DECLARE @Teacher5 INT = (SELECT UserId FROM @TeacherUsers WHERE RowNum = 5);

INSERT INTO dbo.ClassSections(
    SemesterId, CourseId, TeacherId, SectionCode, IsOpen, MaxCapacity, CurrentEnrollment,
    Room, OnlineUrl, Notes
)
SELECT @ActiveSemesterId, c.CourseId, v.TeacherId, v.SectionCode, 1, v.MaxCapacity, 0,
       v.Room, v.OnlineUrl, v.Notes
FROM (VALUES
  (N'PRN222', @Teacher1, N'PRN222-01', 35, N'F301', NULL, N'Core advanced web class'),
  (N'DBI202', @Teacher2, N'DBI202-01', 40, N'B201', NULL, N'Lab-based SQL course'),
  (N'WEB101', @Teacher5, N'WEB101-01', 45, N'A101', NULL, N'Foundation web'),
  (N'SWE102', @Teacher4, N'SWE102-01', 30, N'C402', NULL, N'SE process + UML'),
  (N'AI201',  @Teacher3, N'AI201-01',  30, N'D501', NULL, N'ML fundamentals'),
  (N'MAD101', @Teacher5, N'MAD101-01', 25, N'E202', NULL, N'Android basics')
) v(CourseCode, TeacherId, SectionCode, MaxCapacity, Room, OnlineUrl, Notes)
JOIN dbo.Courses c ON c.CourseCode = v.CourseCode
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ClassSections cs
    WHERE cs.SemesterId = @ActiveSemesterId
      AND cs.CourseId = c.CourseId
      AND cs.SectionCode = v.SectionCode
);

----------------------------------------------------------------
-- 8) Enrollments (>=5) (also respects unique filtered index)
----------------------------------------------------------------
/*
  Enroll each of 5 students into 2 different courses in the active semester
  => 10 enrollment records.
*/
DECLARE @S1 INT = (SELECT UserId FROM @StudentUsers WHERE RowNum = 1);
DECLARE @S2 INT = (SELECT UserId FROM @StudentUsers WHERE RowNum = 2);
DECLARE @S3 INT = (SELECT UserId FROM @StudentUsers WHERE RowNum = 3);
DECLARE @S4 INT = (SELECT UserId FROM @StudentUsers WHERE RowNum = 4);
DECLARE @S5 INT = (SELECT UserId FROM @StudentUsers WHERE RowNum = 5);

DECLARE @CS_PRN222 INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'PRN222');
DECLARE @CS_DBI202 INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'DBI202');
DECLARE @CS_WEB101 INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'WEB101');
DECLARE @CS_SWE102 INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'SWE102');
DECLARE @CS_AI201  INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'AI201');
DECLARE @CS_MAD101 INT = (SELECT cs.ClassSectionId FROM dbo.ClassSections cs
                          JOIN dbo.Courses c ON c.CourseId=cs.CourseId
                          WHERE cs.SemesterId=@ActiveSemesterId AND c.CourseCode=N'MAD101');

-- NOTE: Avoid a CTE here to prevent parser issues in some clients/editors.
-- NOTE: Avoid a CTE here to prevent parser issues in some clients/editors.
INSERT INTO dbo.Enrollments(StudentId, ClassSectionId, SemesterId, CourseId, CreditsSnapshot, Status)
SELECT p.StudentId,
       p.ClassSectionId,
       cs.SemesterId,
       cs.CourseId,
       c.Credits,
       N'ENROLLED'
FROM (VALUES
  (@S1, @CS_PRN222),
  (@S1, @CS_DBI202),
  (@S2, @CS_PRN222),
  (@S2, @CS_WEB101),
  (@S3, @CS_DBI202),
  (@S3, @CS_SWE102),
  (@S4, @CS_AI201),
  (@S4, @CS_WEB101),
  (@S5, @CS_MAD101),
  (@S5, @CS_PRN222)
) AS p(StudentId, ClassSectionId)
JOIN dbo.ClassSections cs ON cs.ClassSectionId = p.ClassSectionId
JOIN dbo.Courses c ON c.CourseId = cs.CourseId
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Enrollments e
    WHERE e.StudentId = p.StudentId
      AND e.CourseId = cs.CourseId
      AND e.SemesterId = cs.SemesterId
      AND e.Status IN (N'ENROLLED', N'WITHDRAWN')
);

-- Keep ClassSections.CurrentEnrollment in sync (optional but nice)
UPDATE cs
SET cs.CurrentEnrollment = x.EnrolledCount
FROM dbo.ClassSections cs
JOIN (
    SELECT ClassSectionId, COUNT(*) AS EnrolledCount
    FROM dbo.Enrollments
    WHERE Status = N'ENROLLED'
    GROUP BY ClassSectionId
) x ON x.ClassSectionId = cs.ClassSectionId;

----------------------------------------------------------------
-- 9) GradeBooks (>=5) (1 per ClassSection)
----------------------------------------------------------------
INSERT INTO dbo.GradeBooks(ClassSectionId, Status, Version, PublishedAt)
SELECT TOP (6) cs.ClassSectionId,
       CASE WHEN ROW_NUMBER() OVER (ORDER BY cs.ClassSectionId) <= 2 THEN N'PUBLISHED' ELSE N'DRAFT' END,
       1,
       CASE WHEN ROW_NUMBER() OVER (ORDER BY cs.ClassSectionId) <= 2 THEN SYSUTCDATETIME() ELSE NULL END
FROM dbo.ClassSections cs
LEFT JOIN dbo.GradeBooks gb ON gb.ClassSectionId = cs.ClassSectionId
WHERE cs.SemesterId = @ActiveSemesterId
  AND gb.GradeBookId IS NULL
ORDER BY cs.ClassSectionId;

----------------------------------------------------------------
-- 10) GradeItems (>=5)
----------------------------------------------------------------
INSERT INTO dbo.GradeItems(GradeBookId, ItemName, MaxScore, Weight, IsRequired, SortOrder)
SELECT gb.GradeBookId, v.ItemName, v.MaxScore, v.Weight, v.IsRequired, v.SortOrder
FROM dbo.GradeBooks gb
CROSS JOIN (VALUES
   (N'Quiz 1',   CAST(10.00 AS DECIMAL(5,2)), CAST(0.2000 AS DECIMAL(6,4)), 1, 1),
   (N'Midterm',  CAST(10.00 AS DECIMAL(5,2)), CAST(0.3000 AS DECIMAL(6,4)), 1, 2),
   (N'Final',    CAST(10.00 AS DECIMAL(5,2)), CAST(0.5000 AS DECIMAL(6,4)), 1, 3)
) v(ItemName, MaxScore, Weight, IsRequired, SortOrder);

----------------------------------------------------------------
-- 11) GradeEntries (>=5)
-- Create entries for each Enrollment x GradeItem of the same ClassSection
----------------------------------------------------------------
INSERT INTO dbo.GradeEntries(GradeItemId, EnrollmentId, Score, UpdatedBy)
SELECT gi.GradeItemId,
       e.EnrollmentId,
       CAST(
          CASE gi.ItemName
            WHEN N'Quiz 1'  THEN 7.5
            WHEN N'Midterm' THEN 8.0
            ELSE 8.5
          END
       AS DECIMAL(5,2)) AS Score,
       cs.TeacherId AS UpdatedBy
FROM dbo.Enrollments e
JOIN dbo.ClassSections cs ON cs.ClassSectionId = e.ClassSectionId
JOIN dbo.GradeBooks gb ON gb.ClassSectionId = cs.ClassSectionId
JOIN dbo.GradeItems gi ON gi.GradeBookId = gb.GradeBookId
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.GradeEntries ge
    WHERE ge.GradeItemId = gi.GradeItemId
      AND ge.EnrollmentId = e.EnrollmentId
);

----------------------------------------------------------------
-- 12) GradeAuditLogs (>=5)
----------------------------------------------------------------
INSERT INTO dbo.GradeAuditLogs(GradeEntryId, ActorUserId, OldScore, NewScore, Reason)
SELECT TOP (5)
    ge.GradeEntryId,
    ge.UpdatedBy,
    ge.Score - 0.5,
    ge.Score,
    N'Initial grading adjustment'
FROM dbo.GradeEntries ge
WHERE ge.Score IS NOT NULL
ORDER BY ge.GradeEntryId;

----------------------------------------------------------------
-- 13) ChatRooms (>=5)
----------------------------------------------------------------
DECLARE @Course_PRN222 INT = (SELECT CourseId FROM dbo.Courses WHERE CourseCode = N'PRN222');
DECLARE @Course_DBI202 INT = (SELECT CourseId FROM dbo.Courses WHERE CourseCode = N'DBI202');
DECLARE @Course_WEB101 INT = (SELECT CourseId FROM dbo.Courses WHERE CourseCode = N'WEB101');

INSERT INTO dbo.ChatRooms(RoomType, CourseId, ClassSectionId, RoomName, Status, CreatedBy)
SELECT v.RoomType, v.CourseId, v.ClassSectionId, v.RoomName, v.Status, v.CreatedBy
FROM (VALUES
 (N'COURSE', @Course_PRN222, NULL, N'PRN222 - Q&A',      N'ACTIVE', @AdminUserId),
 (N'COURSE', @Course_DBI202, NULL, N'DBI202 - Q&A',      N'ACTIVE', @AdminUserId),
 (N'CLASS',  NULL, @CS_PRN222, N'PRN222-01 - Class Chat',N'ACTIVE', @AdminUserId),
 (N'CLASS',  NULL, @CS_DBI202, N'DBI202-01 - Class Chat',N'ACTIVE', @AdminUserId),
 (N'GROUP',  NULL, NULL,       N'Project Team Alpha',   N'ACTIVE', @AdminUserId)
) v(RoomType, CourseId, ClassSectionId, RoomName, Status, CreatedBy)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ChatRooms r WHERE r.RoomName = v.RoomName
);

----------------------------------------------------------------
-- 14) ChatRoomMembers (>=5)
----------------------------------------------------------------
/* Add owner/admin + teacher + a couple students into each room */
DECLARE @Room_PRN222_QA INT = (SELECT TOP (1) RoomId FROM dbo.ChatRooms WHERE RoomName = N'PRN222 - Q&A' ORDER BY RoomId);
DECLARE @Room_DBI202_QA INT = (SELECT TOP (1) RoomId FROM dbo.ChatRooms WHERE RoomName = N'DBI202 - Q&A' ORDER BY RoomId);
DECLARE @Room_PRN222_01 INT = (SELECT TOP (1) RoomId FROM dbo.ChatRooms WHERE RoomName = N'PRN222-01 - Class Chat' ORDER BY RoomId);
DECLARE @Room_DBI202_01 INT = (SELECT TOP (1) RoomId FROM dbo.ChatRooms WHERE RoomName = N'DBI202-01 - Class Chat' ORDER BY RoomId);
DECLARE @Room_TeamA INT = (SELECT TOP (1) RoomId FROM dbo.ChatRooms WHERE RoomName = N'Project Team Alpha' ORDER BY RoomId);

INSERT INTO dbo.ChatRoomMembers(RoomId, UserId, RoleInRoom, MemberStatus)
SELECT v.RoomId, v.UserId, v.RoleInRoom, v.MemberStatus
FROM (VALUES
 (@Room_PRN222_QA, @AdminUserId, N'OWNER',     N'JOINED'),
 (@Room_PRN222_QA, @Teacher1,    N'MODERATOR', N'JOINED'),
 (@Room_PRN222_QA, @S1,          N'MEMBER',    N'JOINED'),
 (@Room_PRN222_QA, @S2,          N'MEMBER',    N'JOINED'),

 (@Room_DBI202_QA, @AdminUserId, N'OWNER',     N'JOINED'),
 (@Room_DBI202_QA, @Teacher2,    N'MODERATOR', N'JOINED'),
 (@Room_DBI202_QA, @S3,          N'MEMBER',    N'JOINED'),

 (@Room_PRN222_01, @Teacher1,    N'OWNER',     N'JOINED'),
 (@Room_PRN222_01, @S1,          N'MEMBER',    N'JOINED'),
 (@Room_PRN222_01, @S5,          N'MEMBER',    N'JOINED'),

 (@Room_DBI202_01, @Teacher2,    N'OWNER',     N'JOINED'),
 (@Room_DBI202_01, @S1,          N'MEMBER',    N'JOINED'),
 (@Room_DBI202_01, @S3,          N'MEMBER',    N'JOINED'),

 (@Room_TeamA,     @S2,          N'OWNER',     N'JOINED'),
 (@Room_TeamA,     @S4,          N'MEMBER',    N'JOINED'),
 (@Room_TeamA,     @S5,          N'MEMBER',    N'JOINED')
) v(RoomId, UserId, RoleInRoom, MemberStatus)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ChatRoomMembers m
    WHERE m.RoomId = v.RoomId
      AND m.UserId = v.UserId
);

----------------------------------------------------------------
-- 15) ChatMessages (>=5)
----------------------------------------------------------------
INSERT INTO dbo.ChatMessages(RoomId, SenderId, MessageType, Content)
VALUES
 (@Room_PRN222_QA, @S1,       N'TEXT', N'Em hỏi phần Repository pattern trong PRN222.'),
 (@Room_PRN222_QA, @Teacher1, N'TEXT', N'Ok, thầy sẽ gửi ví dụ Service + Repository.'),
 (@Room_DBI202_QA, @S3,       N'TEXT', N'Index nào tối ưu cho truy vấn JOIN nhiều bảng?'),
 (@Room_DBI202_QA, @Teacher2, N'TEXT', N'Ưu tiên index cho cột join + filter, xem execution plan.'),
 (@Room_PRN222_01, @AdminUserId, N'SYSTEM', N'Welcome to class chat!'),
 (@Room_TeamA,     @S2,       N'TEXT', N'Mọi người chia task làm module lịch học nha.'),
 (@Room_TeamA,     @S4,       N'TEXT', N'Ok, mình làm UI calendar.'),
 (@Room_TeamA,     @S5,       N'TEXT', N'Mình làm API schedule events.');

----------------------------------------------------------------
-- 16) ChatMessageAttachments (>=5)
----------------------------------------------------------------
INSERT INTO dbo.ChatMessageAttachments(MessageId, FileUrl, FileType, FileSizeBytes)
SELECT TOP (5)
    m.MessageId,
    N'https://example.com/files/' + CAST(m.MessageId AS NVARCHAR(30)) + N'.pdf',
    N'application/pdf',
    120000 + (m.MessageId % 5000)
FROM dbo.ChatMessages m
WHERE m.MessageType = N'TEXT'
ORDER BY m.MessageId;

----------------------------------------------------------------
-- 17) ChatModerationLogs (>=5)
----------------------------------------------------------------
DECLARE @AnyMessageId BIGINT = (SELECT TOP (1) MessageId FROM dbo.ChatMessages ORDER BY MessageId);
INSERT INTO dbo.ChatModerationLogs(RoomId, ActorUserId, Action, TargetUserId, TargetMessageId, MetadataJson)
VALUES
 (@Room_TeamA,     @AdminUserId, N'REPORT',        @S4, NULL,          N'{"reason":"off-topic"}'),
 (@Room_PRN222_QA, @Teacher1,    N'LOCK',          NULL, NULL,         N'{"durationMin":30}'),
 (@Room_PRN222_QA, @Teacher1,    N'UNLOCK',        NULL, NULL,         N'{}'),
 (@Room_DBI202_QA, @AdminUserId, N'DELETE_MESSAGE',NULL, @AnyMessageId, N'{"policy":"spam"}'),
 (@Room_DBI202_01, @Teacher2,    N'REMOVE_MEMBER', @S3, NULL,          N'{"reason":"muted for 1 hour"}');

----------------------------------------------------------------
-- 18) Recurrences (>=5)
----------------------------------------------------------------
INSERT INTO dbo.Recurrences(RRule, StartDate, EndDate)
VALUES
 (N'FREQ=WEEKLY;BYDAY=MO,WE;INTERVAL=1', '2026-01-19','2026-05-10'),
 (N'FREQ=WEEKLY;BYDAY=TU;INTERVAL=1',    '2026-01-20','2026-05-10'),
 (N'FREQ=WEEKLY;BYDAY=TH;INTERVAL=1',    '2026-01-22','2026-05-10'),
 (N'FREQ=WEEKLY;BYDAY=FR;INTERVAL=1',    '2026-01-23','2026-05-10'),
 (N'FREQ=WEEKLY;BYDAY=SA;INTERVAL=2',    '2026-02-07','2026-05-10');

----------------------------------------------------------------
-- 19) ScheduleEvents (>=5)
----------------------------------------------------------------
DECLARE @R1 INT = (SELECT RecurrenceId FROM dbo.Recurrences WHERE RRule LIKE N'FREQ=WEEKLY;BYDAY=MO,WE%');
DECLARE @R2 INT = (SELECT RecurrenceId FROM dbo.Recurrences WHERE RRule LIKE N'FREQ=WEEKLY;BYDAY=TU%');
DECLARE @R3 INT = (SELECT RecurrenceId FROM dbo.Recurrences WHERE RRule LIKE N'FREQ=WEEKLY;BYDAY=TH%');
DECLARE @R4 INT = (SELECT RecurrenceId FROM dbo.Recurrences WHERE RRule LIKE N'FREQ=WEEKLY;BYDAY=FR%');
DECLARE @R5 INT = (SELECT RecurrenceId FROM dbo.Recurrences WHERE RRule LIKE N'FREQ=WEEKLY;BYDAY=SA%');

INSERT INTO dbo.ScheduleEvents(
    ClassSectionId, Title, StartAt, EndAt, Timezone, Location, OnlineUrl,
    TeacherId, Status, RecurrenceId, CreatedBy, UpdatedBy
)
VALUES
 (@CS_PRN222, N'Lecture PRN222 - Week 3', '2026-02-02T01:30:00', '2026-02-02T03:30:00', N'Asia/Ho_Chi_Minh', N'F301', NULL, @Teacher1, N'PUBLISHED', @R1, @AdminUserId, @Teacher1),
 (@CS_DBI202, N'Lecture DBI202 - Week 3', '2026-02-03T01:30:00', '2026-02-03T03:30:00', N'Asia/Ho_Chi_Minh', N'B201', NULL, @Teacher2, N'PUBLISHED', @R2, @AdminUserId, @Teacher2),
 (@CS_WEB101, N'Lecture WEB101 - Week 3', '2026-02-04T01:30:00', '2026-02-04T03:00:00', N'Asia/Ho_Chi_Minh', N'A101', NULL, @Teacher5, N'PUBLISHED', @R1, @AdminUserId, @Teacher5),
 (@CS_SWE102, N'Tutorial SWE102',          '2026-02-05T02:00:00', '2026-02-05T03:30:00', N'Asia/Ho_Chi_Minh', N'C402', NULL, @Teacher4, N'PUBLISHED', @R3, @AdminUserId, @Teacher4),
 (@CS_AI201,  N'Lab AI201',                '2026-02-06T01:00:00', '2026-02-06T03:00:00', N'Asia/Ho_Chi_Minh', N'D501', NULL, @Teacher3, N'PUBLISHED', @R4, @AdminUserId, @Teacher3),
 (@CS_MAD101, N'Workshop MAD101',           '2026-02-07T01:30:00', '2026-02-07T03:30:00', N'Asia/Ho_Chi_Minh', N'E202', NULL, @Teacher5, N'DRAFT',     @R5, @AdminUserId, @Teacher5);

----------------------------------------------------------------
-- 20) ScheduleEventOverrides (>=5)
----------------------------------------------------------------
INSERT INTO dbo.ScheduleEventOverrides(
    RecurrenceId, OriginalDate, OverrideType, NewStartAt, NewEndAt, NewLocation, NewTeacherId, Reason
)
VALUES
 (@R1, '2026-03-02', N'RESCHEDULE', '2026-03-02T03:00:00', '2026-03-02T05:00:00', N'F302', @Teacher1, N'Room conflict'),
 (@R2, '2026-03-03', N'CANCEL',     NULL,                  NULL,                  NULL,   NULL,      N'Holiday'),
 (@R3, '2026-03-05', N'RESCHEDULE', '2026-03-05T01:00:00', '2026-03-05T02:30:00', N'C401', @Teacher4, N'Time adjusted'),
 (@R4, '2026-03-06', N'RESCHEDULE', '2026-03-06T02:00:00', '2026-03-06T04:00:00', N'D502', @Teacher3, N'Lab equipment maintenance'),
 (@R5, '2026-03-07', N'CANCEL',     NULL,                  NULL,                  NULL,   NULL,      N'Instructor unavailable');

----------------------------------------------------------------
-- 21) ScheduleChangeLogs (>=5)
----------------------------------------------------------------
INSERT INTO dbo.ScheduleChangeLogs(ScheduleEventId, ActorUserId, ChangeType, OldJson, NewJson, Reason)
SELECT TOP (5)
    se.ScheduleEventId,
    @AdminUserId,
    N'CREATE',
    NULL,
    N'{"title":"' + se.Title + N'","status":"' + se.Status + N'"}',
    N'Initial publish'
FROM dbo.ScheduleEvents se
ORDER BY se.ScheduleEventId;

----------------------------------------------------------------
-- 22) Notifications (>=5)
----------------------------------------------------------------
INSERT INTO dbo.Notifications(NotificationType, PayloadJson, Status, SentAt)
VALUES
 (N'SCHEDULE_CHANGED', N'{"semester":"2026-SP","note":"Schedule updated"}', N'SENT', SYSUTCDATETIME()),
 (N'GRADE_PUBLISHED',  N'{"semester":"2026-SP","note":"Grades published for some classes"}', N'SENT', SYSUTCDATETIME()),
 (N'CHAT_MENTION',     N'{"room":"PRN222 - Q&A","message":"You were mentioned"}', N'DELIVERED', SYSUTCDATETIME()),
 (N'ENROLLMENT_UPDATE',N'{"semester":"2026-SP","note":"Enrollment status updated"}', N'SENT', SYSUTCDATETIME()),
 (N'AI_SUMMARY_READY', N'{"feature":"score_summary","note":"AI summary is ready"}', N'PENDING', NULL);

----------------------------------------------------------------
-- 23) NotificationRecipients (>=5)
----------------------------------------------------------------
DECLARE @N1 BIGINT = (SELECT MIN(NotificationId) FROM dbo.Notifications);
DECLARE @N2 BIGINT = (SELECT MIN(NotificationId)+1 FROM dbo.Notifications);
DECLARE @N3 BIGINT = (SELECT MIN(NotificationId)+2 FROM dbo.Notifications);
DECLARE @N4 BIGINT = (SELECT MIN(NotificationId)+3 FROM dbo.Notifications);
DECLARE @N5 BIGINT = (SELECT MIN(NotificationId)+4 FROM dbo.Notifications);

INSERT INTO dbo.NotificationRecipients(NotificationId, UserId, DeliveredAt, ReadAt)
VALUES
 (@N1, @S1, SYSUTCDATETIME(), NULL),
 (@N1, @S2, SYSUTCDATETIME(), NULL),
 (@N2, @S3, SYSUTCDATETIME(), SYSUTCDATETIME()),
 (@N3, @Teacher1, SYSUTCDATETIME(), NULL),
 (@N4, @S4, SYSUTCDATETIME(), NULL),
 (@N5, @S5, NULL, NULL);

----------------------------------------------------------------
-- 24) AIChatSessions (>=5)
----------------------------------------------------------------
INSERT INTO dbo.AIChatSessions(UserId, Purpose, ModelName, State, PromptVersion)
VALUES
 (@S1, N'SCORE_SUMMARY',     N'gemini-1.5', N'ACTIVE', N'v1'),
 (@S2, N'STUDY_PLAN',        N'gemini-1.5', N'ACTIVE', N'v1'),
 (@S3, N'COURSE_SUGGESTION', N'gemini-1.5', N'ACTIVE', N'v1'),
 (@S4, N'SCORE_SUMMARY',     N'gemini-1.5', N'ACTIVE', N'v1'),
 (@S5, N'STUDY_PLAN',        N'gemini-1.5', N'ACTIVE', N'v1');

----------------------------------------------------------------
-- 25) AIChatMessages (>=5)
----------------------------------------------------------------
/* 2 messages per session => 10 rows */
INSERT INTO dbo.AIChatMessages(ChatSessionId, SenderType, Content)
SELECT s.ChatSessionId, N'USER',      N'Cho mình tổng kết điểm và môn nên ưu tiên.'
FROM dbo.AIChatSessions s;

INSERT INTO dbo.AIChatMessages(ChatSessionId, SenderType, Content)
SELECT s.ChatSessionId, N'ASSISTANT', N'Mình đã phân tích điểm hiện tại và đề xuất lộ trình học tập.'
FROM dbo.AIChatSessions s;

----------------------------------------------------------------
-- 26) AIToolCalls (>=5)
----------------------------------------------------------------
INSERT INTO dbo.AIToolCalls(ChatSessionId, ToolName, RequestJson, ResponseJson, Status)
SELECT TOP (5)
    s.ChatSessionId,
    N'grade_summary',
    N'{"action":"summarize_grades","semester":"2026-SP"}',
    N'{"ok":true,"summary":"sample"}',
    N'OK'
FROM dbo.AIChatSessions s
ORDER BY s.ChatSessionId;

COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    THROW 51099, @Err, 1;
END CATCH;
GO
