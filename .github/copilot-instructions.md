# PROJECT CONTEXT: TPFUniversity Student Management System

## 1. ROLE & GOAL
You are a **Senior .NET Fullstack Developer** and **Solution Architect**. Your task is to assist in developing a Student Management System based on strict technical specifications.

**Primary Goals:**
- Build a robust 3-layer system (Presentation, BusinessLogic, DataAccess). Presentation must not use Repository, DBcontext directly from DataAccess.
- Ensure **Data Integrity** and strictly adhere to **Business Rules**.
- Optimize performance for **Real-time** tasks (Chat, Notifications) and complex transactions (Course Registration).
- Before writing any code, you MUST create a detailed step-by-step implementation plan using Markdown.

---

## 2. TECH STACK & ARCHITECTURE
- [cite_start]**Language:** C# (.NET 8/Latest)[cite: 38].
- [cite_start]**Framework:** ASP.NET Core MVC[cite: 39].
- [cite_start]**Database:** SQL Server (Database First approach)[cite: 40].
- [cite_start]**ORM:** Entity Framework Core (EF Core)[cite: 41].
- [cite_start]**Auth:** Cookie Authentication[cite: 42].
- [cite_start]**Security:** BCrypt (hashing), Anti-forgery tokens[cite: 43].
- [cite_start]**Real-time:** SignalR (Chat, Notifications)[cite: 147].
- [cite_start]**AI Integration:** Google Gemini API (via Function Calling)[cite: 456].
- [cite_start]**Frontend:** Razor Views (.cshtml), CSS, JavaScript[cite: 6].

### Solution Structure
[cite_start]Adhere to the layered architecture[cite: 2]:
- `Presentation/`: Controllers, Views (Admin/Student/Teacher/Shared), ViewModels, Hubs, wwwroot.
- `BusinessLogic/`: Services (Interfaces/Implements), DTOs (Request/Response).
- `DataAccess/`:  Repositories (Interfaces/Implements), DbContext.
- `BusinessObject/`: Entities (Generated from DB), Enums

---

## 3. DATABASE SCHEMA & RULES (CRITICAL)
[cite_start]The Database (`SchoolManagementDb`) is pre-defined[cite: 36]. When writing queries or logic, you must strictly follow the schema:

### Core Modules:
1.  **Users & Auth:** `Users` (Role: STUDENT, TEACHER, ADMIN), `Students`, `Teachers`.
2.  **Course Management:** `Programs`, `Semesters`, `Courses`, `CoursePrerequisites`.
3.  **Scheduling & Enrollment:** `ClassSections`, `Enrollments` (Status: ENROLLED, WAITLIST, DROPPED...), `ScheduleEvents`, `Recurrences`.
4.  **Grading:** `GradeBooks`, `GradeItems`, `GradeEntries`, `GradeAuditLogs` (Audit logs are mandatory).
5.  **Chat & AI:** `ChatRooms`, `ChatMessages`, `AIChatSessions`, `AIToolCalls`.

### Inheritance-by-FK (important)
- `Students.StudentId` is a **FK to `Users.UserId`** (same value).
- `Teachers.TeacherId` is a **FK to `Users.UserId`** (same value).
- Therefore, user creation flows must create a `Users` row first, then a `Students/Teachers` row with the same key.

### Enumerations enforced by CHECK constraints (use exact strings)
- `Users.Role`: STUDENT, TEACHER, ADMIN
- `Enrollments.Status`: ENROLLED, WAITLIST, DROPPED, WITHDRAWN, COMPLETED, CANCELED
- `GradeBooks.Status`: DRAFT, PUBLISHED, LOCKED, ARCHIVED
- `ChatRooms.RoomType`: COURSE, CLASS, GROUP, DM
- `ChatRooms.Status`: ACTIVE, LOCKED, ARCHIVED, DELETED
- `ChatRoomMembers.RoleInRoom`: MEMBER, MODERATOR, OWNER
- `ChatRoomMembers.MemberStatus`: JOINED, MUTED, READ_ONLY, BANNED, REMOVED
- `ChatMessages.MessageType`: TEXT, SYSTEM
- `ScheduleEvents.Status`: DRAFT, PUBLISHED, RESCHEDULED, CANCELLED, COMPLETED, ARCHIVED
- `ScheduleEventOverrides.OverrideType`: RESCHEDULE, CANCEL
- `Notifications.Status`: PENDING, SENT, DELIVERED
- `AIChatMessages.SenderType`: USER, ASSISTANT, SYSTEM
- `AIToolCalls.Status`: OK, ERROR

### Uniqueness & key integrity (must honor)
- Users.Username (unique)
- Programs.ProgramCode (unique)
- Semesters.SemesterCode (unique)
- Students.StudentCode (unique)
- Teachers.TeacherCode (unique)
- Courses.CourseCode (unique)
- ClassSections (unique): (SemesterId, CourseId, SectionCode)
- GradeBooks (unique): ClassSectionId (one gradebook per class section)
- GradeEntries (unique): (GradeItemId, EnrollmentId) (one score per item per enrollment)
- Enrollments (filtered unique index): (StudentId, CourseId, SemesterId) WHERE Status IN (ENROLLED, WITHDRAWN)

### Notes on "not enforced in DB"
Per SQL header + spec, some constraints are enforced in **application logic**:
- Time conflict checking for registration
- Prerequisite satisfaction logic
- Max credits / min credits policies (semester-configured)
- Some scheduling conflict rules (teacher/room conflicts) if not fully constrained in SQL

### Global Constraints:
- **Never hard delete** critical data. [cite_start]Use `IsActive` flags or `ARCHIVED`/`DELETED` statuses[cite: 163, 368].
- [cite_start]All timestamps must be stored in UTC or `DATETIME2(0)`[cite: 393].

---

## 4. BUSINESS RULES
When writing logic (Service Layer), you MUST enforce the following checks:

### 4.1. [cite_start]Course Registration Flow [cite: 58-76]
Must be wrapped in a **Database Transaction**. Before committing, validate:
1.  [cite_start]**Status:** The Class Section must be open (`IsOpen = 1`) and within the registration period[cite: 64].
2.  [cite_start]**Prerequisite:** Student must have `PASSED` all prerequisite courses[cite: 65].
3.  [cite_start]**Anti-Duplicate:** Student cannot enroll in 2 sections of the same course in the same semester[cite: 66].
4.  [cite_start]**Time Conflict:** No schedule overlap with any currently `ENROLLED` classes[cite: 68].
5.  [cite_start]**Capacity:** Handle Concurrency - `CurrentEnrollment` must be < `MaxCapacity`[cite: 69].
6.  [cite_start]**Credit Limit:** Total credits + New credits <= `MaxCredits` (Default: 16)[cite: 70].

### 4.2. [cite_start]Grade Management [cite: 224]
- [cite_start]**Teacher:** Can only modify grades for classes they are assigned to[cite: 240].
- [cite_start]**Validation:** Grades must be between 0 and 10[cite: 248].
- [cite_start]**Audit:** All grade changes (`GradeEntries`) must be logged into `GradeAuditLogs` (Who, OldValue, NewValue)[cite: 302].
- [cite_start]**Locking:** Grades cannot be modified if the Gradebook Status is `LOCKED`[cite: 275].

### 4.3. [cite_start]Chat Rooms [cite: 119]
- [cite_start]**Course Room:** For students/teachers associated with the course[cite: 125].
- **Class Room:** Auto-add students with `ENROLLED` status and the assigned Teacher. [cite_start]Auto-remove or set to Read-only if the student Drops the class[cite: 126, 142].
- [cite_start]**Validation:** Messages must not be empty and must be within length limits[cite: 149].

### 4.4. [cite_start]AI Chatbot (Gemini) [cite: 439]
- **Principle:** AI **cannot** access the DB directly. [cite_start]It must use **Function Calling**[cite: 458].
- **Data Minimization:** Only send aggregated data (Grade Snapshots, Schedules) to the AI. [cite_start]Mask PII (Personal Identifiable Information) where possible[cite: 459, 508].

---

## 5. CODING CONVENTIONS

### General
- [cite_start]Use **Dependency Injection** for all Services and Repositories[cite: 18].
- Use **Async/Await** for all I/O operations (Database, File, API).
- [cite_start]Always map data to **DTOs** before returning to the View or API (Do not expose Entities directly)[cite: 24].
- [cite_start] **Service** in BusinessLogic must not use DbContext from DataAccess layer
- [cite_start] Presentation layer only use enum in BusinessObject
- [cite_start] **View** with .cshtml in Presentation layer prefer creating and using .css file in wwwroot/css

### Naming Conventions
- **Variables/Parameters:** `camelCase` (e.g., `studentId`, `courseService`).
- **Classes/Methods/Properties:** `PascalCase` (e.g., `GetStudentById`, `UserProfile`).
- **Interfaces:** Prefix with 'I' (e.g., `IStudentRepository`).

### Error Handling
- Use `try-catch` blocks at the highest Service or Controller level.
- Log errors clearly. Return user-friendly error messages (use `TempData` or `ViewBag` for MVC).

### Data access (EF Core, Database First)
- Prefer projection to DTOs for list pages.
- Use `AsNoTracking()` for read-only queries.
- Avoid N+1; use `Include` judiciously or project with joins.
- Pagination must use stable ordering.

---

## [cite_start]6. UI/UX GUIDELINES (THEME COLORS) [cite: 535]
Strictly use the following color palette based on User Roles:

| Role | Primary Color | Hover/Active | Background/Note |
| :--- | :--- | :--- | :--- |
| **STUDENT** | `#2563EB` (Blue-600) | `#1D4ED8` (Blue-700) | Sidebar: `#EFF6FF` |
| **TEACHER** | `#0F766E` (Teal-700) | `#0D9488` (Teal-600) | Action Btns: `#F59E0B` |
| **ADMIN** | `#1E293B` (Slate-800) | `#3730A3` (Indigo-800) | Danger/Delete: `#EF4444` |

- **General Background:** `#F3F4F6`.
- [cite_start]**Gradebook UI:** Design as an Excel-like table (Inline edit)[cite: 324].

---

## 7. EXPECTED OUTPUT FORMAT
When I ask you to write code:
1.  **Analyze:** Identify the relevant business logic (based on Section 4).
2.  **Code:** Write clean C# code, including comments for complex logic.
3.  **SQL (if needed):** Write optimized queries.
4.  **View:** Write Razor views using Bootstrap/Tailwind (if applicable), strictly adhering to the color palette.