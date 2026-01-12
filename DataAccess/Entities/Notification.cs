using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int? SenderUserId { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Notification")]
    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    [ForeignKey("SenderUserId")]
    [InverseProperty("Notifications")]
    public virtual User? SenderUser { get; set; }
}
