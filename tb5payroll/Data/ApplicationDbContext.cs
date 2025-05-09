using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using tb5payroll.Models;

namespace tb5payroll.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for your entities
        public DbSet<EmployeeData> EmployeeData { get; set; }
        public DbSet<EmployeeArchive> EmployeeArchive { get; set; }
        public DbSet<TimeSetData> TimeSetData { get; set; }


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
                entity.Property(e => e.CalculatedPayEmployeeData).HasColumnType("decimal(10,2)");
                entity.Property(e => e.LateDeductionEmployeeData).HasColumnType("decimal(7,2)");
                entity.Property(e => e.UnderTimeEmployeeData).HasColumnType("decimal(7,2)");
                entity.Property(e => e.TaxEmployeeData).HasColumnType("decimal(10,2)");
            });

            // Configure the EmployeeArchive table
            modelBuilder.Entity<EmployeeArchive>(entity =>
            {
                entity.HasKey(e => e.ArchiveId);
                entity.Property(e => e.ArchiveId).HasColumnName("archiveId");
                entity.Property(e => e.IdEmployeeData).HasColumnName("idEmployeeData");
                entity.Property(e => e.EmployeeArchiveDate).HasColumnName("employeeArchiveDate").HasColumnType("datetime");
                entity.Property(e => e.BirthdayEmployeeData).HasColumnName("birthdayEmployeeData");
                entity.Property(e => e.NameEmployeeData).HasColumnName("nameEmployeeData");
                entity.Property(e => e.BasePayEmployeeData).HasColumnName("basePayEmployeeData");
                entity.Property(e => e.HoursWorkedEmployeeData).HasColumnName("hoursWorkedEmployeeData").HasColumnType("decimal(4,1)");
                entity.Property(e => e.OvertimeHoursEmployeeData).HasColumnName("overtimeHoursEmployeeData").HasColumnType("decimal(4,1)");
                entity.Property(e => e.HolidayHoursEmployeeData).HasColumnName("holidayHoursEmployeeData").HasColumnType("decimal(4,1)");
                entity.Property(e => e.TrainingEmployeeData).HasColumnName("trainingEmployeeData").HasColumnType("decimal(7,2)");
                entity.Property(e => e.CashAdvEmployeeData).HasColumnName("cashAdvEmployeeData").HasColumnType("decimal(7,2)");
                entity.Property(e => e.LoanEmployeeData).HasColumnName("loanEmployeeData").HasColumnType("decimal(7,2)");
                entity.Property(e => e.SssEmployeeData).HasColumnName("sssEmployeeData").HasColumnType("decimal(6,2)");
                entity.Property(e => e.PagIbigEmployeeData).HasColumnName("pagIbigEmployeeData").HasColumnType("decimal(6,2)");
                entity.Property(e => e.PhilHealthEmployeeData).HasColumnName("philHealthEmployeeData").HasColumnType("decimal(6,2)");
                entity.Property(e => e.TaxEmployeeData).HasColumnName("taxEmployeeData").HasColumnType("decimal(10,2)");
                entity.Property(e => e.CalculatedPayEmployeeData).HasColumnName("calculatedPayEmployeeData").HasColumnType("decimal(10,2)");
            });
            modelBuilder.Entity<TimeSetData>(entity =>
            {
                entity.HasKey(e => e.TimeSet);
                entity.Property(e => e.IdEmployeeData).IsRequired();
                entity.Property(e => e.UndertimeThreshold).HasColumnType("int");
                entity.Property(e => e.OvertimeThreshold).HasColumnType("int");
                entity.Property(e => e.EarlyOutThreshold).HasColumnType("int");
                entity.Property(e => e.HalfdayMultiplier).HasColumnType("decimal(5,2)");
                entity.Property(e => e.OvertimeMultiplier).HasColumnType("decimal(5,2)");
            });
        }
    }
}
