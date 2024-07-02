using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExceptionService.Models;

public partial class WorkflowException
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string Type { get; set; } = null!;

    [Column(TypeName = "text")]
    public string? Data { get; set; }

    public bool? IsBusinessError { get; set; }

    [Column(TypeName = "text")]
    public string? ErrorInfo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? EmpName { get; set; }

    public int? EmpNumber { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? BranchName { get; set; }

    public int? BranchNumber { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Enroute { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? OnSite { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Clear { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? LockedBy { get; set; }

    public long? JobNumber { get; set; }

    public int? JobSeqNumber { get; set; }
}
