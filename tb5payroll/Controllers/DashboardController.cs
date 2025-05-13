using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using tb5payroll.Data;
using tb5payroll.Models;

namespace tb5payroll.Controllers;

    [Authorize]
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
        // Validate request contains a file
        if (Request.Form.Files.Count == 0)
        {
            return BadRequest("No file was uploaded.");
        }

        var file = Request.Form.Files[0];
        
        // Validate file
        if (file.Length == 0)
        {
            return BadRequest("The uploaded file is empty.");
        }

        // Validate Excel file extension
        var validExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!validExtensions.Contains(fileExtension))
        {
            return BadRequest("Please upload a valid Excel file (.xlsx or .xls)");
        }

        // Configure EPPlus (critical for proper operation)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        var sheets = new List<string>();
        
        using (var memoryStream = new MemoryStream())
        {
            // Copy file to memory stream
            await file.CopyToAsync(memoryStream);
            
            // Reset position - this is often the missing piece!
            memoryStream.Position = 0;
            
            try
            {
                using (var package = new ExcelPackage(memoryStream))
                {
                    // Validate workbook
                    if (package.Workbook == null || package.Workbook.Worksheets.Count == 0)
                    {
                        return BadRequest("The Excel file contains no worksheets.");
                    }
                    
                    // Collect sheet names
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        if (!string.IsNullOrWhiteSpace(worksheet.Name))
                        {
                            sheets.Add(worksheet.Name);
                        }
                    }
                    
                    if (sheets.Count == 0)
                    {
                        return BadRequest("No valid worksheets found in the Excel file.");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Invalid Excel file: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing Excel file: {ex.Message}");
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
            decimal overtimePay = (employee.OvertimeHoursEmployeeData ?? 0) * basePay; // Overtime should be multiplied by base pay
            decimal holidayPay = (employee.HolidayHoursEmployeeData ?? 0) * basePay; // Holiday pay should be multiplied by base pay
            
            // Deductions
            decimal sss = employee.SssEmployeeData ?? 0;
            decimal pagibig = employee.PagIbigEmployeeData ?? 0;
            decimal loans = employee.LoanEmployeeData ?? 0;
            decimal cashAdvance = employee.CashAdvEmployeeData ?? 0;
            decimal philhealth = employee.PhilHealthEmployeeData ?? 0;
            decimal tax = employee.TaxEmployeeData ?? 0;
            decimal lateDeduction = employee.LateDeductionEmployeeData ?? 0;
            decimal underTimeDeduction = employee.UnderTimeEmployeeData ?? 0;

            // Calculate gross pay (basic pay + additions)
            decimal grossPay = (basePay * hoursWorked) + trainingPay + overtimePay + holidayPay;
            
            // Calculate total deductions
            decimal totalDeductions = sss + pagibig + loans + cashAdvance + philhealth + tax + lateDeduction + underTimeDeduction;
            
            // Calculate net pay (gross pay - deductions)
            decimal netPay = grossPay - totalDeductions;

            return Json(new 
            {
                name = employee.NameEmployeeData,
                nameEmployeeData = employee.NameEmployeeData,
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
                tax,
                lateDeduction,
                underTimeDeduction,
                grossPay,
                totalDeductions,
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

[HttpGet]
public async Task<IActionResult> GetManualData()
{
    try
    {
        // This will be your manual mode implementation
        // For now, just returning sample structure
        return Json(new { 
            success = true, 
            message = "Manual mode data loaded",
            data = new List<object>() // Your manual data will go here
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { 
            success = false, 
            message = "Error loading manual data",
            error = ex.Message 
        });
    }
}

[HttpPost]
public async Task<IActionResult> SaveManualData([FromBody] ManualEmployeeData data)
{
    try
    {
        if (int.TryParse(data.Id, out int birthdayId))
        {
            var employee = await _context.EmployeeData
                .FirstOrDefaultAsync(e => e.BirthdayEmployeeData == birthdayId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Update fields from manual input
            if (decimal.TryParse(data.HoursWorked, out decimal hours))
            {
                employee.HoursWorkedEmployeeData = hours;
            }
            
            // Add similar updates for other fields (overtime, holiday, etc.)
            
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Invalid employee ID" });
    }
    catch (Exception ex)
    {
        return Json(new { 
            success = false, 
            message = "Error saving manual data",
            error = ex.Message 
        });
    }
}

public class ManualEmployeeData
{
    public string Id { get; set; }
    public string Workday { get; set; }
    public string Holiday { get; set; }
    public string Overtime { get; set; }
    public string HoursWorked { get; set; }
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
