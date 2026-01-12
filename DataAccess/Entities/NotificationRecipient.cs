using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

[PrimaryKey("NotificationId", "ReceiverUserId")]
public partial class NotificationRecipient
{
    [Key]
    public int NotificationId { get; set; }

    [Key]
    public int ReceiverUserId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    [ForeignKey("NotificationId")]
    [InverseProperty("NotificationRecipients")]
    public virtual Notification Notification { get; set; } = null!;

    [ForeignKey("ReceiverUserId")]
    [InverseProperty("NotificationRecipients")]
    public virtual User ReceiverUser { get; set; } = null!;
}
