using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using tb5payroll.Data;

namespace tb5payroll.Controllers
{
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
                            var id = worksheet.Cells[row, 1].Text; // Excel ID (birthday)
                            var name = "Not Found"; // Default value if no match or parsing fails

                            // Get the values from specific columns
                            var hoursWorked = worksheet.Cells[row, 5].Text; // Column E (5)
                            var overtime = worksheet.Cells[row, 10].Text; // Column J (10)
                            var workday = worksheet.Cells[row, 12].Text; // Column L (12)
                            var holiday = worksheet.Cells[row, 11].Text; // Column K (11) - New holiday data

                            // Attempt to parse the ID as an integer (birthday)
                            if (int.TryParse(id, out int birthdayId))
                            {
                                // Query database for matching BirthdayEmployeeData
                                var employeeData = await _context.EmployeeData
                                    .FirstOrDefaultAsync(e => e.BirthdayEmployeeData == birthdayId);

                                // If a matching record is found, update the name and database fields
                                if (employeeData != null)
                                {
                                    name = employeeData.NameEmployeeData;
                                    
                                    // Update database fields if values exist
                                    if (decimal.TryParse(hoursWorked, out decimal hoursWorkedValue))
                                    {
                                        employeeData.HoursWorkedEmployeeData = hoursWorkedValue;
                                    }
                                    
                                    if (decimal.TryParse(overtime, out decimal overtimeValue))
                                    {
                                        employeeData.OvertimeHoursEmployeeData = overtimeValue;
                                    }
                                    
                                    // Update holiday hours from column K
                                    if (decimal.TryParse(holiday, out decimal holidayValue))
                                    {
                                        employeeData.HolidayHoursEmployeeData = holidayValue;
                                    }
                                    
                                    // Save changes to the database
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    // Log or handle the case where no matching record is found
                                    name = "No matching record in database";
                                }
                            }
                            else
                            {
                                // Handle the case where the ID cannot be parsed as an integer
                                name = "Invalid ID format";
                            }

                            // Add the employee to the list
                            employees.Add(new Employee
                            {
                                Id = id,
                                Name = name,
                                Workday = workday, 
                                Holiday = decimal.TryParse(holiday, out decimal hd) ? (int)hd : 0, // Updated to use holiday data
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
}