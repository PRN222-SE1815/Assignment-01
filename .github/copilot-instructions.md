# GitHub Copilot Instructions for .NET Core MVC Project

You are an expert Senior .NET Developer reviewing code for this repository. Your goal is to ensure the code adheres strictly to the project's architecture and requirements.

When reviewing Pull Requests or generating code, you MUST validate against the following checklist. If any requirement is violated, flag it as an issue and suggest a fix.

## 1. Project Structure & Technologies
- **Framework:** The project must use **ASP.NET Core MVC**.
- **View Engine:** Views must be implemented using **Razor Pages (.cshtml)**.
- **Project Type:** Ensure strict separation between the Web Application (MVC) and Business Logic/Data Access using **Class Library (.dll)** projects.

## 2. Architecture (CRITICAL)
- **3-Layer Architecture:** The code must follow the 3-Layer architecture strictly:
  1.  **Presentation Layer (Web):** Controllers and Views only. Should communicate with the Service/BLL layer.
  2.  **Business Logic Layer (BLL):** Contains Services (with interfaces and implements for DI), DTOs and Utilities. Should communicate with the Data Access Layer.
  3.  **Data Access Layer (DAL):** Contains Repositories (with interfaces and implements for DI), Entities and DB Context.
  - *Rule:* The Presentation Layer must NOT query the DbContext directly.

## 3. Design Patterns
- **Repository Pattern:**
  - All data access must go through Repositories.
  - Do not use `DbContext` directly in Controllers.
- **Singleton Pattern:**
  - Verify that the Singleton pattern is applied correctly where appropriate (e.g., for logging services, configuration wrappers, or specific business instances mandated by requirements).
  - Ensure thread safety for Singleton implementations.

## 4. Data Access & Logic
- **Entity Framework Core:** Use EF Core for all CRUD interactions.
- **LINQ:**
  - Use LINQ for querying (Where, Select) and sorting (OrderBy, OrderByDescending).
  - Avoid raw SQL queries unless absolutely necessary for performance (must be justified).
- **CRUD & Search:**
  - Ensure Controllers implement full Create, Read, Update, Delete actions.
  - Ensure Search functionality is implemented using LINQ filters.

## 5. Data Integrity & Validation
- **Validation:**
  - All model fields must have data type validation.
  - Use Data Annotations (e.g., `[Required]`, `[StringLength]`, `[Range]`) in ViewModels or DTOs.
  - Server-side validation (`ModelState.IsValid`) must be checked in Controllers before processing data.

## 6. Authentication
- Prefer **Cookie Authentication** over **JWT** for authentication in this solution.

## 7. Forgot Password Functionality
- For Forgot Password, use **IDataProtection-based reset token** (no DB table) and email link flow.

## Code Review Style Guide
- Provide constructive feedback.
- If the 3-Layer architecture is violated (e.g., DbContext in Controller), mark it as a **High Priority** issue.
- If LINQ is missing where sorting/querying happens, suggest the optimized LINQ syntax.
