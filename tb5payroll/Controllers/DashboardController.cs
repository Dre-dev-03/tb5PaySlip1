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

// Add this new model class to DashboardController.cs
public class ManualModeRequest
{
    public IFormFile File { get; set; }
    public List<string> SelectedSheets { get; set; } = new List<string>();
}

// Add these new endpoints to DashboardController.cs
[HttpPost]
public async Task<IActionResult> GetManualSheets([FromForm] ManualModeRequest request)
{
    try
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded." });
        }

        var sheets = new List<string>();

        using (var stream = new MemoryStream())
        {
            await request.File.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    sheets.Add(sheet.Name);
                }
            }
        }

        return Json(new { success = true, sheets });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = "Error loading sheets", error = ex.Message });
    }
}


            private void CalculateEmployeeTotals(EmployeeDTRData employee)
            {
                employee.totalRegularHours = 0;
                employee.totalOvertimeMinutes = 0;
                employee.totalUndertimeMinutes = 0;

                foreach (var record in employee.Records)
                {
                    var calculation = CalculateDailyHours(record);
                    record.regularHours = calculation.regularHours;
                    record.overtimeMinutes = calculation.overtimeMinutes;
                    record.undertimeMinutes = calculation.undertimeMinutes;

                    employee.totalRegularHours += calculation.regularHours;
                    employee.totalOvertimeMinutes += calculation.overtimeMinutes;
                    employee.totalUndertimeMinutes += calculation.undertimeMinutes;
                }
    
                // Convert to hours for display
                employee.totalOvertimeHours = employee.totalOvertimeMinutes / 60.0;
            }
private EmployeeDTRData ExtractEmployeeDTR(ExcelWorksheet worksheet, int employeeIndex)
{
    // Define column mappings for each employee (0, 1, 2)
    var columnMappings = new[]
    {
        new {
            IdCell = "J5",
            DatePeriodCell = "B5",
            TimeInCols = new[] { "B", "C" },
            TimeOutCols = new[] { "D", "E", "F" },
            OvertimeInCols = new[] { "K", "L" },
            OvertimeOutCols = new[] { "M", "N" }
        },
        new {
            IdCell = "Y5",
            DatePeriodCell = "Q5",
            TimeInCols = new[] { "Q", "R" },
            TimeOutCols = new[] { "S", "T", "U" },
            OvertimeInCols = new[] { "Z", "AA" },
            OvertimeOutCols = new[] { "AB", "AC" }
        },
        new {
            IdCell = "AN5",
            DatePeriodCell = "AF5",
            TimeInCols = new[] { "AF", "AG" },
            TimeOutCols = new[] { "AH", "AI", "AJ" },
            OvertimeInCols = new[] { "AO", "AP" },
            OvertimeOutCols = new[] { "AQ", "AR" }
        }
    };

    var mapping = columnMappings[employeeIndex];

    // Get employee ID and date period
    string employeeId = worksheet.Cells[mapping.IdCell].Text.Trim();
    string datePeriod = worksheet.Cells[mapping.DatePeriodCell].Text.Trim();

    if (string.IsNullOrEmpty(employeeId)) return null;

    // Parse date period
    var dates = ParseDatePeriod(datePeriod);
    if (dates == null || dates.Count == 0) return null;

    var records = new List<DTRRecord>();
    int maxRowToCheck = Math.Min(43, 13 + dates.Count - 1);

    for (int row = 13; row <= maxRowToCheck; row++)
    {
        // Skip blank rows after row 28 if no data
        if (row > 28 && IsRowEmpty(worksheet, row, mapping)) continue;

        var timeIn = GetMergedCellValue(worksheet, mapping.TimeInCols[0] + row, mapping.TimeInCols.Last() + row);
        var timeOut = GetMergedCellValue(worksheet, mapping.TimeOutCols[0] + row, mapping.TimeOutCols.Last() + row);
        var overtimeIn = GetMergedCellValue(worksheet, mapping.OvertimeInCols[0] + row, mapping.OvertimeInCols.Last() + row);
        var overtimeOut = GetMergedCellValue(worksheet, mapping.OvertimeOutCols[0] + row, mapping.OvertimeOutCols.Last() + row);

        // Skip if all time fields are empty
        if (string.IsNullOrWhiteSpace(timeIn) && 
            string.IsNullOrWhiteSpace(timeOut) && 
            string.IsNullOrWhiteSpace(overtimeIn) && 
            string.IsNullOrWhiteSpace(overtimeOut)) continue;

        records.Add(new DTRRecord
        {
            Date = dates[row - 13].ToString("yyyy-MM-dd"),
            TimeIn = timeIn,
            TimeOut = timeOut,
            OvertimeIn = overtimeIn,
            OvertimeOut = overtimeOut
        });
    }

    return new EmployeeDTRData
    {
        EmployeeId = employeeId,
        DatePeriod = datePeriod,
        Records = records
    };
}

private List<DateTime> ParseDatePeriod(string datePeriod)
{
    if (string.IsNullOrWhiteSpace(datePeriod)) return null;

    var parts = datePeriod.Split('~');
    if (parts.Length != 2) return null;

    if (DateTime.TryParse(parts[0], out DateTime startDate) && 
        DateTime.TryParse(parts[1], out DateTime endDate))
    {
        var dates = new List<DateTime>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            dates.Add(date);
        }
        return dates;
    }

    return null;
}

private bool IsRowEmpty(ExcelWorksheet worksheet, int row, dynamic mapping)
{
    var timeIn = GetMergedCellValue(worksheet, mapping.TimeInCols[0] + row, mapping.TimeInCols.Last() + row);
    var timeOut = GetMergedCellValue(worksheet, mapping.TimeOutCols[0] + row, mapping.TimeOutCols.Last() + row);
    var overtimeIn = GetMergedCellValue(worksheet, mapping.OvertimeInCols[0] + row, mapping.OvertimeInCols.Last() + row);
    var overtimeOut = GetMergedCellValue(worksheet, mapping.OvertimeOutCols[0] + row, mapping.OvertimeOutCols.Last() + row);

    return string.IsNullOrWhiteSpace(timeIn) && 
           string.IsNullOrWhiteSpace(timeOut) && 
           string.IsNullOrWhiteSpace(overtimeIn) && 
           string.IsNullOrWhiteSpace(overtimeOut);
}

private string GetMergedCellValue(ExcelWorksheet worksheet, string startCell, string endCell)
{
    var cell = worksheet.Cells[startCell + ":" + endCell];
    return cell.Merge ? cell.First().Text : worksheet.Cells[startCell].Text;
}

[HttpPost]
public IActionResult RenderDTRResults([FromBody] List<SheetDTRData> data)
{
    return PartialView("_ManualDTRResults", data);
}

[HttpPost]
public async Task<IActionResult> ProcessManualData([FromForm] ManualModeRequest request)
{
    try
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { success = false, message = "No file uploaded." });
        }

        if (request.SelectedSheets == null || request.SelectedSheets.Count == 0)
        {
            return BadRequest(new { success = false, message = "No sheets selected." });
        }

        var result = new List<SheetDTRData>();

        using (var stream = new MemoryStream())
        {
            await request.File.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                foreach (var sheetName in request.SelectedSheets)
                {
                    var worksheet = package.Workbook.Worksheets[sheetName];
                    if (worksheet == null) continue;

                    var sheetData = new SheetDTRData
                    {
                        SheetName = sheetName,
                        Employees = new List<EmployeeDTRData>()
                    };

                    for (int empIndex = 0; empIndex < 3; empIndex++)
                    {
                        var empData = ExtractEmployeeDTR(worksheet, empIndex);
                        if (empData != null)
                        {
                            // Calculate and set the time values
                            CalculateEmployeeTotals(empData);
                            sheetData.Employees.Add(empData);
                        }
                    }

                    result.Add(sheetData);
                }

                return Json(new { success = true, data = result });
            }
        }
    } catch (Exception ex)
         {
             return StatusCode(500, new { 
                 success = false, 
                 message = "Error processing DTR data", 
                 error = ex.Message 
             });
         }
}

// Add these classes to DashboardController.cs
public class SheetDTRData
{
    public string SheetName { get; set; }
    public List<EmployeeDTRData> Employees { get; set; }
}

public class EmployeeDTRData
{
    public string EmployeeId { get; set; }
    public string DatePeriod { get; set; }
    public List<DTRRecord> Records { get; set; }
    
    public double totalRegularHours { get; set; }
    public int totalOvertimeMinutes { get; set; }
    public int totalUndertimeMinutes { get; set; }
    public double totalOvertimeHours { get; set; }
}

public class DTRRecord
{
    public string Date { get; set; }
    public string TimeIn { get; set; }
    public string TimeOut { get; set; }
    public string OvertimeIn { get; set; }
    public string OvertimeOut { get; set; }
    public double regularHours { get; set; }
    public int overtimeMinutes { get; set; }
    public int undertimeMinutes { get; set; }
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
