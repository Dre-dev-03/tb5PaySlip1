using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using tb5payroll.Models;

namespace tb5payroll.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add DbSet for EmployeeData
    public DbSet<EmployeeData> EmployeeData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the EmployeeData table
        modelBuilder.Entity<EmployeeData>(entity =>
        {
            entity.HasKey(e => e.IdEmployeeData);
            entity.Property(e => e.IdEmployeeData).IsRequired();
            entity.Property(e => e.BirthdayEmployeeData).IsRequired();
            entity.Property(e => e.NameEmployeeData).IsRequired();
            entity.Property(e => e.BasePayEmployeeData).IsRequired();
            entity.Property(e => e.HoursWorkedEmployeeData).HasColumnType("decimal(4,1)");
            entity.Property(e => e.OvertimeHoursEmployeeData).HasColumnType("decimal(4,1)");
            entity.Property(e => e.HolidayHoursEmployeeData).HasColumnType("decimal(4,1)");
            entity.Property(e => e.TrainingEmployeeData).HasColumnType("decimal(7,2)");
            entity.Property(e => e.CashAdvEmployeeData).HasColumnType("decimal(7,2)");
            entity.Property(e => e.LoanEmployeeData).HasColumnType("decimal(7,2)");
            entity.Property(e => e.SssEmployeeData).HasColumnType("decimal(6,2)");
            entity.Property(e => e.PagIbigEmployeeData).HasColumnType("decimal(6,2)");
            entity.Property(e => e.PhilHealthEmployeeData).HasColumnType("decimal(6,2)");
        });
    }
}