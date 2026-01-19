using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Conversation
{
    [Key]
    public int ConversationId { get; set; }

    public bool? IsGroup { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime? CreatedAt { get; set; }
    //add new field CourseId
    public int? CourseId { get; set; }

    [InverseProperty("Conversation")]
    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    [InverseProperty("Conversation")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    //navigation property for Course
    [ForeignKey("CourseId")]
    [InverseProperty("Conversations")]
    public virtual Course? Course { get; set; }
}
