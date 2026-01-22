using BusinessLogic.Interfaces.AI;
using BusinessLogic.Services.Implements.AI;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBDefault")));

//AI
builder.Services.Configure<GeminiConfig>(
    builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient<IOpenAiService, GeminiService>();
// Business service
builder.Services.AddScoped<IStudentAnalysisService, StudentAnalysisService>();


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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
