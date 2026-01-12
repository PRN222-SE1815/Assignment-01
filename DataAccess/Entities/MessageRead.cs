using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

[PrimaryKey("MessageId", "UserId")]
public partial class MessageRead
{
    [Key]
    public int MessageId { get; set; }

    [Key]
    public int UserId { get; set; }

    public DateTime? ReadAt { get; set; }

    [ForeignKey("MessageId")]
    [InverseProperty("MessageReads")]
    public virtual Message Message { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("MessageReads")]
    public virtual User User { get; set; } = null!;
}
