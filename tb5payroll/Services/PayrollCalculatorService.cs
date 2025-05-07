using tb5payroll.Models;

namespace tb5payroll.Services
{
    public class PayrollCalculatorService : IPayrollCalculatorService
    {
        public PayrollResult CalculatePayroll(EmployeeData employee)
        {
            decimal basePay = employee.BasePayEmployeeData;
            decimal hoursWorked = employee.HoursWorkedEmployeeData ?? 0;
            decimal trainingPay = employee.TrainingEmployeeData ?? 0;
            decimal overtimePay = (employee.OvertimeHoursEmployeeData ?? 0) * basePay;
            decimal holidayPay = (employee.HolidayHoursEmployeeData ?? 0) * basePay;

            decimal grossPay = (basePay * hoursWorked) + trainingPay + overtimePay + holidayPay;

            decimal totalDeductions =
                (employee.SssEmployeeData ?? 0) +
                (employee.PhilHealthEmployeeData ?? 0) +
                (employee.PagIbigEmployeeData ?? 0) +
                (employee.LoanEmployeeData ?? 0) +
                (employee.TaxEmployeeData ?? 0) +
                (employee.CashAdvEmployeeData ?? 0) +
                (employee.LateDeductionEmployeeData ?? 0) +
                (employee.UnderTimeEmployeeData ?? 0);

            return new PayrollResult
            {
                GrossPay = grossPay,
                TotalDeductions = totalDeductions,
                NetPay = grossPay - totalDeductions,
                OvertimePay = overtimePay,
                HolidayPay = holidayPay
            };
        }

        public PayrollResult CalculatePayroll(EmployeeArchive archive)
        {
            // Reuse the same logic for archives
            var employeeData = new EmployeeData
            {
                BasePayEmployeeData = archive.BasePayEmployeeData,
                HoursWorkedEmployeeData = archive.HoursWorkedEmployeeData,
                TrainingEmployeeData = archive.TrainingEmployeeData,
                OvertimeHoursEmployeeData = archive.OvertimeHoursEmployeeData,
                HolidayHoursEmployeeData = archive.HolidayHoursEmployeeData,
                SssEmployeeData = archive.SssEmployeeData,
                PhilHealthEmployeeData = archive.PhilHealthEmployeeData,
                PagIbigEmployeeData = archive.PagIbigEmployeeData,
                LoanEmployeeData = archive.LoanEmployeeData,
                TaxEmployeeData = archive.TaxEmployeeData,
                CashAdvEmployeeData = archive.CashAdvEmployeeData,
                IdEmployeeData = null,
                NameEmployeeData = null
                //LateDeductionEmployeeData = archive.LateDeductionEmployeeData,
                //UnderTimeEmployeeData = archive.UnderTimeEmployeeData
            };

            return CalculatePayroll(employeeData);
        }
    }
}