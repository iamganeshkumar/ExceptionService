using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSOpsExceptionService.Models;

public partial class LastWorkFlowException
{
    [Key]
    public Guid Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }
}
