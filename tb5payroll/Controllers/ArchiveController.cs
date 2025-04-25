using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tb5payroll.Data;
using tb5payroll.Models;
using System.Globalization;

namespace tb5payroll.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArchiveController(ApplicationDbContext context)
        {
            _context = context;
        }

         public async Task<IActionResult> Archive()
                {
                    return View();
                }

        [HttpGet]
        public async Task<IActionResult> GetArchivedData(int? month, int? year, string searchTerm = "")
        {
            try
            {
                var query = _context.EmployeeArchive.AsQueryable();

                // Apply month filter if provided
                if (month.HasValue && month > 0)
                {
                    query = query.Where(e => e.EmployeeArchiveDate.Month == month);
                }

                // Apply year filter if provided
                if (year.HasValue && year > 0)
                {
                    query = query.Where(e => e.EmployeeArchiveDate.Year == year);
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(e => 
                        e.NameEmployeeData.ToLower().Contains(searchTerm) || 
                        e.IdEmployeeData.ToLower().Contains(searchTerm));
                }

                var archivedEmployees = await query
                    .OrderByDescending(e => e.EmployeeArchiveDate)
                    .ThenBy(e => e.NameEmployeeData)
                    .Select(e => new ArchivedEmployeeViewModel
                    {
                        ArchiveId = e.ArchiveId,
                        EmployeeId = e.IdEmployeeData,
                        Name = e.NameEmployeeData,
                        Department = "TB5 STAFF", // Default or get from related table
                        ArchiveDate = e.EmployeeArchiveDate.ToString("yyyy-MM-dd"),
                        HolidayHours = e.HolidayHoursEmployeeData ?? 0,
                        OvertimeHours = e.OvertimeHoursEmployeeData ?? 0,
                        HoursWorked = e.HoursWorkedEmployeeData ?? 0,
                        GrossPay = e.CalculatedPayEmployeeData ?? 0,
                        BasePay = e.BasePayEmployeeData ?? 0
                    })
                    .ToListAsync();

                return Json(new { success = true, data = archivedEmployees });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArchiveYears()
        {
            try
            {
                var years = await _context.EmployeeArchive
                    .Select(e => e.EmployeeArchiveDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToListAsync();

                return Json(new { success = true, data = years });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        public class ArchivedEmployeeViewModel
        {
            public string ArchiveId { get; set; }
            public string EmployeeId { get; set; }
            public string Name { get; set; }
            public string Department { get; set; }
            public string ArchiveDate { get; set; }
            public decimal HolidayHours { get; set; }
            public decimal OvertimeHours { get; set; }
            public decimal HoursWorked { get; set; }
            public decimal BasePay { get; set; }
            public decimal GrossPay { get; set; }
        }
    }
}