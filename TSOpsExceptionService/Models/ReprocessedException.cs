using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSOpsExceptionService.Models;

public partial class ReprocessedException
{
    [Key]
    public Guid Id { get; set; }

    public long? JobNumber { get; set; }

    public int? JobSequenceNo { get; set; }
    public string? Type { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ReprocessedDateTime { get; set; }
}
