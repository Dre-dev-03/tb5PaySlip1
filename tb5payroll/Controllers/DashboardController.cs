using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using tb5payroll.Data;
using tb5payroll.Models;

namespace tb5payroll.Controllers;


    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetInitialData()
        {
            try
            {
                var employees = await _context.EmployeeData
                    .Where(e => e.HoursWorkedEmployeeData != null) // Only include employees with data
                    .Select(e => new Employee
                    {
                        Id = e.BirthdayEmployeeData.ToString(),
                        Name = e.NameEmployeeData ?? "Unknown",
                        Workday = "", // Default value since this comes from Excel
                        Holiday = (int)(e.HolidayHoursEmployeeData ?? 0),
                        Overtime = (int)(e.OvertimeHoursEmployeeData ?? 0),
                        HoursWorked = (int)(e.HoursWorkedEmployeeData ?? 0)
                    })
                    .ToListAsync();

                if (!employees.Any())
                {
                    return Json(new { success = false, message = "No employee data found" });
                }

                return Json(new { success = true, data = employees });
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging here)
                return StatusCode(500, new { success = false, message = "Error loading initial data", error = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> GetSheets()
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var sheets = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        foreach (var sheet in package.Workbook.Worksheets)
                        {
                            sheets.Add(sheet.Name);
                        }
                    }
                }

                return Json(sheets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Update the GetData method to use the correct EmployeeData model
[HttpPost]
public async Task<IActionResult> GetData(string sheetName)
{
    try
    {
        var file = Request.Form.Files[0];
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var employees = new List<Employee>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[sheetName];
                if (worksheet == null)
                {
                    return NotFound("Sheet not found.");
                }

                int startRow = 5;
                for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
                {
                    var id = worksheet.Cells[row, 1].Text;
                    var name = "Not Found";

                    // Get values from Excel
                    var hoursWorked = worksheet.Cells[row, 5].Text;
                    var overtime = worksheet.Cells[row, 10].Text;
                    var workday = worksheet.Cells[row, 12].Text;
                    var holiday = worksheet.Cells[row, 11].Text;

                    if (int.TryParse(id, out int birthdayId))
                    {
                        // Use fully qualified name for EmployeeData
                        var employeeData = await _context.EmployeeData
                            .FirstOrDefaultAsync(e => e.BirthdayEmployeeData == birthdayId);

                        if (employeeData != null)
                        {
                            name = employeeData.NameEmployeeData;
                            
                            // Update database fields
                            if (decimal.TryParse(hoursWorked, out decimal hoursWorkedValue))
                            {
                                employeeData.HoursWorkedEmployeeData = hoursWorkedValue;
                            }
                            
                            if (decimal.TryParse(overtime, out decimal overtimeValue))
                            {
                                employeeData.OvertimeHoursEmployeeData = overtimeValue;
                            }
                            
                            if (decimal.TryParse(holiday, out decimal holidayValue))
                            {
                                employeeData.HolidayHoursEmployeeData = holidayValue;
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                    }

                    employees.Add(new Employee
                    {
                        Id = id,
                        Name = name,
                        Workday = workday, 
                        Holiday = decimal.TryParse(holiday, out decimal hd) ? (int)hd : 0,
                        Overtime = decimal.TryParse(overtime, out decimal ov) ? (int)ov : 0,
                        HoursWorked = decimal.TryParse(hoursWorked, out decimal hw) ? (int)hw : 0
                    });
                }
            }
        }

        return Json(employees);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

[HttpGet]
public async Task<IActionResult> GetEmployeeDetails(string id)
{
    try
    {
        if (int.TryParse(id, out int birthdayId))
        {
            var employee = await _context.EmployeeData
                .FirstOrDefaultAsync(e => e.BirthdayEmployeeData == birthdayId);

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            // Calculate all pay components
            decimal basePay = employee.BasePayEmployeeData;
            decimal hoursWorked = employee.HoursWorkedEmployeeData ?? 0;
            decimal trainingPay = employee.TrainingEmployeeData ?? 0;
            decimal overtimePay = employee.OvertimeHoursEmployeeData ?? 0;
            decimal holidayPay = employee.HolidayHoursEmployeeData ?? 0;
            decimal sss = employee.SssEmployeeData ?? 0;
            decimal pagibig = employee.PagIbigEmployeeData ?? 0;
            decimal loans = employee.LoanEmployeeData ?? 0;
            decimal cashAdvance = employee.CashAdvEmployeeData ?? 0;
            decimal philhealth = employee.PhilHealthEmployeeData ?? 0;
            decimal tax = employee.TaxEmployeeData ?? 0;
            decimal grossPay = (basePay * hoursWorked) + trainingPay + overtimePay + holidayPay;
            decimal deductions = sss + pagibig + loans + cashAdvance + philhealth + tax;
            decimal netPay = grossPay - deductions;

            return Json(new 
            {
                name = employee.NameEmployeeData, // Ensure this matches what the frontend expects
                nameEmployeeData = employee.NameEmployeeData, // Alternative field name
                holidayPay,
                overtimePay,
                hoursWorked,
                basePay,
                trainingPay,
                sss,
                pagibig,
                loans,
                cashAdvance,
                philhealth,
                grossPay,
                tax,
                netPay
            });
        }
        return BadRequest("Invalid employee ID");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}
[HttpPost]
public async Task<IActionResult> ArchiveCurrentData()
{
    try
    {
        // Get all current employees with all fields
        var currentEmployees = await _context.EmployeeData
            .AsNoTracking()
            .ToListAsync();

        if (!currentEmployees.Any())
        {
            return Json(new { success = false, message = "No employees found to archive" });
        }

        var archiveEntries = new List<EmployeeArchive>();
        var archiveDate = DateTime.Now;

        foreach (var employee in currentEmployees)
        {
            var archiveEntry = new EmployeeArchive
            {
                // Required fields
                ArchiveId = Guid.NewGuid().ToString(),
                IdEmployeeData = employee.IdEmployeeData ?? throw new Exception("Missing IdEmployeeData"),
                EmployeeArchiveDate = archiveDate,
                NameEmployeeData = employee.NameEmployeeData ?? "Unknown",
                
                // Personal information
                BirthdayEmployeeData = employee.BirthdayEmployeeData,
                
                // Pay information
                BasePayEmployeeData = employee.BasePayEmployeeData,
                HoursWorkedEmployeeData = employee.HoursWorkedEmployeeData,
                OvertimeHoursEmployeeData = employee.OvertimeHoursEmployeeData,
                HolidayHoursEmployeeData = employee.HolidayHoursEmployeeData,
                TrainingEmployeeData = employee.TrainingEmployeeData,
                
                // Deductions
                CashAdvEmployeeData = employee.CashAdvEmployeeData,
                LoanEmployeeData = employee.LoanEmployeeData,
                SssEmployeeData = employee.SssEmployeeData,
                PagIbigEmployeeData = employee.PagIbigEmployeeData,
                PhilHealthEmployeeData = employee.PhilHealthEmployeeData,
                TaxEmployeeData = employee.TaxEmployeeData, 
                CalculatedPayEmployeeData = employee.CalculatedPayEmployeeData,
                //LateDeductionEmployeeData = employee.LateDeductionEmployeeData,
                //UnderTimeEmployeeData = employee.UnderTimeEmployeeData
            };

            archiveEntries.Add(archiveEntry);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.EmployeeArchive.AddRangeAsync(archiveEntries);
            int recordsSaved = await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { 
                success = true,
                message = $"Successfully archived {recordsSaved} employee records",
                count = recordsSaved
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw; // This will be caught by the outer try-catch
        }
    }
    catch (Exception ex)
    {
        // Get the root cause error
        var rootEx = ex;
        while (rootEx.InnerException != null) 
        {
            rootEx = rootEx.InnerException;
        }
        
        return Json(new {
            success = false,
            message = $"Archive failed: {rootEx.Message}",
            errorDetails = ex.ToString()
        });
    }
}
        public class Employee
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Workday { get; set; } 
            public int Holiday { get; set; }
            public int Overtime { get; set; }
            public int HoursWorked { get; set; }
            
            
        }
    }
