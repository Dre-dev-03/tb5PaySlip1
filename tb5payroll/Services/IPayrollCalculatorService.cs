using tb5payroll.Models;

namespace tb5payroll.Services
{
    public interface IPayrollCalculatorService
    {
        PayrollResult CalculatePayroll(EmployeeData employee);
        PayrollResult CalculatePayroll(EmployeeArchive archive);
    }

    public class PayrollResult
    {
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal HolidayPay { get; set; }
    }
}