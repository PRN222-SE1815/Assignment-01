using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Message
{
    [Key]
    public int MessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderUserId { get; set; }

    public string Body { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public DateTime? EditedAt { get; set; }

    public bool? IsDeleted { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("Messages")]
    public virtual Conversation Conversation { get; set; } = null!;

    [InverseProperty("Message")]
    public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();
}
