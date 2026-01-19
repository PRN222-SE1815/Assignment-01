using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class SchoolManagementDbContext : DbContext
{
    public SchoolManagementDbContext(DbContextOptions<SchoolManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseSchedule> CourseSchedules { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageRead> MessageReads { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D87700DF12E6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsGroup).HasDefaultValue(false);
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ConversationId, e.UserId }).HasName("PK__Conversa__112854B30782DD01");

            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__Conve__6D0D32F4");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__UserI__6E01572D");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A72182E054");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Courses).HasConstraintName("FK__Courses__Teacher__59FA5E80");
        });

        modelBuilder.Entity<CourseSchedule>(entity =>
        {
            entity.HasKey(e => e.CourseScheduleId).HasName("PK__CourseSc__1678FC2B579F82F2");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseSchedules_Courses");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BA9F6F61D");

            entity.Property(e => e.EnrollDate).HasDefaultValueSql("(CONVERT([date],sysdatetime()))");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Cours__628FA481");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Stude__619B8048");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grades__54F87A57730F7B93");

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Grades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Grades__Enrollme__656C112C");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9CC7104A11");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__Conver__72C60C4A");
        });

        modelBuilder.Entity<MessageRead>(entity =>
        {
            entity.HasKey(e => new { e.MessageId, e.UserId }).HasName("PK__MessageR__190480587113527D");

            entity.Property(e => e.ReadAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageReads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MessageRe__Messa__76969D2E");

            entity.HasOne(d => d.User).WithMany(p => p.MessageReads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MessageRe__UserI__778AC167");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12B2E53BF0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.Notifications).HasConstraintName("FK__Notificat__Sende__7B5B524B");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.NotificationId, e.ReceiverUserId }).HasName("PK__Notifica__C6BCFBB73904E424");

            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Notif__7F2BE32F");

            entity.HasOne(d => d.ReceiverUser).WithMany(p => p.NotificationRecipients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Recei__00200768");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A38AF7956");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B998706C4D7");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__UserId__5629CD9C");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF25964CAA91A38");

            entity.HasOne(d => d.User).WithMany(p => p.Teachers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teachers__UserId__534D60F1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C6DE2B4BF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK__Users__RoleId__5070F446");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
