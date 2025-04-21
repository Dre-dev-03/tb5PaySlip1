using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tb5payroll.Models;

public class TimeSetData
{
    [Key]
    [Column("timeSetData")]
    public required string TimeSet { get; set; }
    
    [ForeignKey(nameof(EmployeeData))]
    [Column("idEmployeeData")]
    public required string IdEmployeeData { get; set; }
    
    [Column("undertimeThreshold")]
    public required int? UndertimeThreshold { get; set; }
    
    [Column("overtimeThreshold")]
    public required int? OvertimeThreshold { get; set; }

    [Column("earlyoutThreshold")]
    public required int? EarlyOutThreshold { get; set; }
    
    [Column("overtimeMultiplier")]
    public required decimal? OvertimeMultiplier { get; set; }
    
    [Column("halfdayMultiplier")]
    public required decimal? HalfdayMultiplier { get; set; }
    
}