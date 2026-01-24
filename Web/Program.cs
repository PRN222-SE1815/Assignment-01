using BusinessLogic.DTOs.Settings;
using BusinessLogic.Interfaces.AI;
using BusinessLogic.Services.Implements;
using BusinessLogic.Services.Implements.AI;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Implements;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Database
builder.Services.AddDbContext<SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

//AI
builder.Services.Configure<GeminiConfig>(
    builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<IOpenAiService, GeminiService>();
// Business service
builder.Services.AddScoped<IStudentAnalysisService, StudentAnalysisService>();

// Register Repositories
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IUserRepository, DataAccess.Repositories.Implements.UserRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IConversationRepository, DataAccess.Repositories.Implements.ConversationRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IConversationParticipantRepository, DataAccess.Repositories.Implements.ConversationParticipantRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IMessageRepository, DataAccess.Repositories.Implements.MessageRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.INotificationRepository, DataAccess.Repositories.Implements.NotificationRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.ICourseRepository, DataAccess.Repositories.Implements.CourseRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IEnrollmentRepository, DataAccess.Repositories.Implements.EnrollmentRepository>();
builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IStudentRepository, DataAccess.Repositories.Implements.StudentRepository>();

// Register Services
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IAuthService, BusinessLogic.Services.Implements.AuthService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IUserService, BusinessLogic.Services.Implements.UserService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IConversationService, BusinessLogic.Services.Implements.ConversationService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IMessageService, BusinessLogic.Services.Implements.MessageService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.INotificationService, BusinessLogic.Services.Implements.NotificationService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.ICourseConversationService, BusinessLogic.Services.Implements.CourseConversationService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IStudyGroupService, BusinessLogic.Services.Implements.StudyGroupService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IEnrollmentServiceForChat, BusinessLogic.Services.Implements.EnrollmentServiceForChat>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.ICourseService, BusinessLogic.Services.Implements.CourseService>();

builder.Services.AddDataProtection();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();

// Add SignalR
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<Web.Hubs.ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
