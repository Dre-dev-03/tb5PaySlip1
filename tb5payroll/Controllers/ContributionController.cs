using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tb5payroll.Data;
using tb5payroll.Models;

namespace tb5payroll.Controllers
{
    public class ContributionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContributionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Contribution()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _context.EmployeeData
                .Select(e => new 
                {
                    id = e.IdEmployeeData,
                    name = e.NameEmployeeData,
                    birthday = e.BirthdayEmployeeData
                })
                .ToListAsync();

            return Json(employees);
        }
        
              [HttpPost]
public async Task<IActionResult> ApplyContributions([FromBody] ContributionModel model)
{
    try
    {
        if (model == null)
        {
            return BadRequest("Invalid contribution data");
        }

        var employee = await _context.EmployeeData.FindAsync(model.EmployeeId);
        if (employee == null)
        {
            return NotFound("Employee not found");
        }

        // Update employee contributions
        if (model.WHTax.HasValue) employee.TaxEmployeeData = model.WHTax.Value;
        if (model.PagIbig.HasValue) employee.PagIbigEmployeeData = model.PagIbig.Value;
        if (model.SSS.HasValue) employee.SssEmployeeData = model.SSS.Value;
        if (model.PhilHealth.HasValue) employee.PhilHealthEmployeeData = model.PhilHealth.Value; // Add this line
        if (model.Tardiness.HasValue) employee.CashAdvEmployeeData = model.Tardiness.Value;
        if (model.Loan.HasValue) employee.LoanEmployeeData = model.Loan.Value;
        if (model.Others.HasValue) employee.TrainingEmployeeData = model.Others.Value;

        await _context.SaveChangesAsync();
        return Ok();
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

[HttpPost]
public async Task<IActionResult> ApplyToAll([FromBody] ContributionModel model)
{
    try
    {
        if (model == null)
        {
            return BadRequest("Invalid contribution data");
        }

        var allEmployees = await _context.EmployeeData.ToListAsync();
        
        foreach (var employee in allEmployees)
        {
            if (model.WHTax.HasValue) employee.TaxEmployeeData = model.WHTax.Value;
            if (model.PagIbig.HasValue) employee.PagIbigEmployeeData = model.PagIbig.Value;
            if (model.SSS.HasValue) employee.SssEmployeeData = model.SSS.Value;
            if (model.PhilHealth.HasValue) employee.PhilHealthEmployeeData = model.PhilHealth.Value; // Add this line
            if (model.Tardiness.HasValue) employee.CashAdvEmployeeData = model.Tardiness.Value;
            if (model.Loan.HasValue) employee.LoanEmployeeData = model.Loan.Value;
            if (model.Others.HasValue) employee.TrainingEmployeeData = model.Others.Value;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

 

        [HttpPost]
        public async Task<IActionResult> SearchEmployees(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            var employees = await _context.EmployeeData
                .Where(e => e.NameEmployeeData.Contains(searchTerm))
                .Select(e => new 
                {
                    id = e.IdEmployeeData,
                    name = e.NameEmployeeData,
                    birthday = e.BirthdayEmployeeData
                })
                .ToListAsync();

            return Json(employees);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetEmployeeContributions(string id)
        {
            var employee = await _context.EmployeeData.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = employee.IdEmployeeData,
                name = employee.NameEmployeeData,
                sssEmployeeData = employee.SssEmployeeData,
                pagIbigEmployeeData = employee.PagIbigEmployeeData,
                philHealthEmployeeData = employee.PhilHealthEmployeeData,
                cashAdvEmployeeData = employee.CashAdvEmployeeData,
                loanEmployeeData = employee.LoanEmployeeData,
                trainingEmployeeData = employee.TrainingEmployeeData,
                taxEmployeeData = employee.TaxEmployeeData
            });
        }
        public class ContributionModel
        {
            public string EmployeeId { get; set; }
            public decimal? WHTax { get; set; }
            public decimal? PagIbig { get; set; }
            public decimal? SSS { get; set; }
            public decimal? PhilHealth { get; set; } 
            public decimal? Tardiness { get; set; }
            public decimal? Loan { get; set; }
            public decimal? Others { get; set; }
            

        }
    }
}