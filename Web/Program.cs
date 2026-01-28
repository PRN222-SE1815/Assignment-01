using BusinessLogic.Services.Implements;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.Settings;
using DataAccess;
using DataAccess.Repositories.Implements;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Web.Hubs;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddDbContext<SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI for repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IClassSectionRepository, ClassSectionRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IPrerequisiteRepository, PrerequisiteRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IGradebookRepository, GradebookRepository>();
builder.Services.AddScoped<IGradeItemRepository, GradeItemRepository>();
builder.Services.AddScoped<IGradeEntryRepository, GradeEntryRepository>();
builder.Services.AddScoped<IGradeAuditLogRepository, GradeAuditLogRepository>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatRoomMemberRepository, ChatRoomMemberRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IChatMessageAttachmentRepository, ChatMessageAttachmentRepository>();
builder.Services.AddScoped<IChatModerationLogRepository, ChatModerationLogRepository>();
builder.Services.AddScoped<IAIChatSessionRepository, AIChatSessionRepository>();
builder.Services.AddScoped<IAIChatMessageRepository, AIChatMessageRepository>();
builder.Services.AddScoped<IAIToolCallRepository, AIToolCallRepository>();

// DI for services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<INotificationPublisher, NotificationPublisher>();
builder.Services.AddScoped<IGradebookService, GradebookService>();
builder.Services.AddScoped<IStudentGradeService, StudentGradeService>();
builder.Services.AddScoped<IChatService, ChatService>();

// AI Chatbot services
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>();
builder.Services.AddScoped<IAIChatService, AIChatService>();

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "SchoolManagement.Auth";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Account/AccessDenied");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<ChatHub>("/hubs/chat");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
