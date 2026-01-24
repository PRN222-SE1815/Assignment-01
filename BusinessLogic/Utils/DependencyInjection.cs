using BusinessLogic.Services.Implements;
using BusinessLogic.Services.Interfaces;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Repositories.Implements;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Utils;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ DbContext (DAL)
        services.AddDbContext<SchoolManagementDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // ✅ Repositories (DAL)
        services.AddScoped<IGradeRepository, GradeRepository>();

        // ✅ Services (BLL)
        services.AddScoped<IGradeService, GradeService>();

        return services;
    }
}
