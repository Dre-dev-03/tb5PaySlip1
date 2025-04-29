using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tb5payroll.Data;
using tb5payroll.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace tb5payroll.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArchiveController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Archive()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetArchiveData(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                IQueryable<EmployeeArchive> query = _context.EmployeeArchive
                    .OrderByDescending(e => e.EmployeeArchiveDate);

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    query = query.Where(e => e.EmployeeArchiveDate >= startDate);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(e => e.EmployeeArchiveDate <= endDate.Value.AddDays(1)); // Include entire end day
                }

                var archiveData = await query
                    .Select(e => new 
                    {
                        e.ArchiveId,
                        e.BirthdayEmployeeData,
                        e.NameEmployeeData,
                        e.EmployeeArchiveDate,
                        e.HoursWorkedEmployeeData,
                        e.OvertimeHoursEmployeeData,
                        e.HolidayHoursEmployeeData,
                        e.BasePayEmployeeData,
                        e.CalculatedPayEmployeeData,
                        e.SssEmployeeData,
                        e.PhilHealthEmployeeData,
                        e.PagIbigEmployeeData,
                        e.LoanEmployeeData,
                        e.TaxEmployeeData
                    })
                    .ToListAsync();

                if (!archiveData.Any())
                {
                    return Json(new { success = false, message = "No archive records found" });
                }

                return Json(new { success = true, data = archiveData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error loading archive data", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArchiveDetails(string id, string date)
        {
            try
            {
                if (int.TryParse(id, out int birthdayId) && DateTime.TryParse(date, out DateTime archiveDate))
                {
                    var archiveRecord = await _context.EmployeeArchive
                        .FirstOrDefaultAsync(e => e.BirthdayEmployeeData == birthdayId && 
                                                 e.EmployeeArchiveDate.Date == archiveDate.Date);

                    if (archiveRecord == null)
                    {
                        return NotFound("Archive record not found");
                    }

                    return Json(new 
                    {
                        nameEmployeeData = archiveRecord.NameEmployeeData,
                        birthdayEmployeeData = archiveRecord.BirthdayEmployeeData,
                        employeeArchiveDate = archiveRecord.EmployeeArchiveDate,
                        basePayEmployeeData = archiveRecord.BasePayEmployeeData,
                        hoursWorkedEmployeeData = archiveRecord.HoursWorkedEmployeeData,
                        overtimeHoursEmployeeData = archiveRecord.OvertimeHoursEmployeeData,
                        holidayHoursEmployeeData = archiveRecord.HolidayHoursEmployeeData,
                        sssEmployeeData = archiveRecord.SssEmployeeData,
                        philHealthEmployeeData = archiveRecord.PhilHealthEmployeeData,
                        pagIbigEmployeeData = archiveRecord.PagIbigEmployeeData,
                        loanEmployeeData = archiveRecord.LoanEmployeeData,
                        taxEmployeeData = archiveRecord.TaxEmployeeData,
                        calculatedPayEmployeeData = archiveRecord.CalculatedPayEmployeeData
                    });
                }
                return BadRequest("Invalid parameters");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteArchiveRecord([FromBody] string archiveId)
        {
            try
            {
                if (string.IsNullOrEmpty(archiveId))
                {
                    return BadRequest(new { success = false, message = "Archive ID is required" });
                }

                var record = await _context.EmployeeArchive
                    .FirstOrDefaultAsync(e => e.ArchiveId == archiveId);

                if (record == null)
                {
                    return NotFound(new { success = false, message = "Archive record not found" });
                }

                _context.EmployeeArchive.Remove(record);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Archive record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error deleting archive record", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOldRecords(DateTime cutoffDate)
        {
            try
            {
                var oldRecords = await _context.EmployeeArchive
                    .Where(e => e.EmployeeArchiveDate < cutoffDate)
                    .ToListAsync();

                if (!oldRecords.Any())
                {
                    return Json(new { success = false, message = "No records found older than the specified date" });
                }

                _context.EmployeeArchive.RemoveRange(oldRecords);
                int recordsDeleted = await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Successfully deleted {recordsDeleted} old archive records",
                    count = recordsDeleted
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error deleting old archive records", 
                    error = ex.Message 
                });
            }
        }
    }
}