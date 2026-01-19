using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

[Index("Username", Name = "UQ__Users__536C85E4BCAC30FE", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D10534B09FBA3E", IsUnique = true)]
public partial class User
{
    [Key]
    public int UserId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [StringLength(100)]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? RoleId { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    [InverseProperty("User")]
    public virtual ICollection<MessageRead> MessageReads { get; set; } = new List<MessageRead>();

    [InverseProperty("ReceiverUser")]
    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    [InverseProperty("SenderUser")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role? Role { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    [InverseProperty("User")]
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
