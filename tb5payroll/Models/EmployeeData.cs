using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tb5payroll.Models
{
    public class EmployeeData
    {
        [Key]
        [Column("idEmployeeData")]
        public required string IdEmployeeData { get; set; }

        [Column("birthdayEmployeeData")]
        public int BirthdayEmployeeData { get; set; }

        [Column("nameEmployeeData")]
        public required string NameEmployeeData { get; set; }

        [Column("basePayEmployeeData")]
        public decimal BasePayEmployeeData { get; set; }

        [Column("hoursWorkedEmployeeData")]
        public decimal? HoursWorkedEmployeeData { get; set; }

        [Column("overtimeHoursEmployeeData")]
        public decimal? OvertimeHoursEmployeeData { get; set; }

        [Column("holidayHoursEmployeeData")]
        public decimal? HolidayHoursEmployeeData { get; set; }

        [Column("trainingEmployeeData")]
        public decimal? TrainingEmployeeData { get; set; }

        [Column("cashAdvEmployeeData")]
        public decimal? CashAdvEmployeeData { get; set; }

        [Column("loanEmployeeData")]
        public decimal? LoanEmployeeData { get; set; }

        [Column("sssEmployeeData")]
        public decimal? SssEmployeeData { get; set; }

        [Column("pagIbigEmployeeData")]
        public decimal? PagIbigEmployeeData { get; set; }

        [Column("philHealthEmployeeData")]
        public decimal? PhilHealthEmployeeData { get; set; }
        
        [Column("taxEmployeeData")]
        public decimal? TaxEmployeeData { get; set; }
        
        [Column("calculatedPayEmployeeData")]
        public decimal? CalculatedPayEmployeeData { get; set; }
        
        [Column("lateDeductionEmployeeData")]
        public decimal? LateDeductionEmployeeData { get; set; }
        
        [Column("underTimeDeductionEmployeeData")]
        public decimal? UnderTimeEmployeeData { get; set; }
    }
}