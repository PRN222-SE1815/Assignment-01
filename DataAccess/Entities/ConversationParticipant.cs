using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

[PrimaryKey("ConversationId", "UserId")]
public partial class ConversationParticipant
{
    [Key]
    public int ConversationId { get; set; }

    [Key]
    public int UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("ConversationParticipants")]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ConversationParticipants")]
    public virtual User User { get; set; } = null!;
}
