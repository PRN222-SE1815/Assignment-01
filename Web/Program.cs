using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataAccess.Entities.SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataAccess.Repositories.Interfaces.IUserRepository, DataAccess.Repositories.Implements.UserRepository>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IAuthService, BusinessLogic.Services.Implements.AuthService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IUserService, BusinessLogic.Services.Implements.UserService>();

builder.Services.AddDataProtection();
builder.Services.Configure<BusinessLogic.DTOs.Settings.SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IEmailService, BusinessLogic.Services.Implements.MailKitEmailService>();
builder.Services.AddScoped<BusinessLogic.Services.Interfaces.IForgotPasswordService, BusinessLogic.Services.Implements.ForgotPasswordService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
