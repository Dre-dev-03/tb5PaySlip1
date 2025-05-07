using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using tb5payroll.Data;
using tb5payroll.Services;

namespace tb5payroll;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Database configuration
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Identity configuration
        builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Add your custom services HERE (before builder.Build())
        builder.Services.AddScoped<IPayrollCalculatorService, PayrollCalculatorService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddControllersWithViews();

        // Configure Excel license
        ExcelPackage.License.SetNonCommercialPersonal("The Big Five Training and Assessment Center");

        var app = builder.Build();  // ← This makes the services read-only!

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}