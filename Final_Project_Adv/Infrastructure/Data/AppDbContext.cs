using Final_Project_Adv.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Adv.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }
    public DbSet<Subtask> Subtask { get; set; }
    public DbSet<TaskItem> TaskItem { get; set; }
    public DbSet<Department> Department { get; set; }
    public DbSet<TaskAssignment> TaskAssignment { get; set; }
    public DbSet<TaskComment> TaskComment { get; set; }
    public DbSet<SubtaskComment> SubtaskComment { get; set; }
    public DbSet<AuditLog> AuditLog { get; set; }
    public DbSet<Schedule> Schedule { get; set; }
    public DbSet<ScheduleParticipant> ScheduleParticipant { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Subtask → AssignedTo (optional, no cascade) ──────────────────────
        modelBuilder.Entity<Subtask>()
            .HasOne(s => s.AssignedTo)
            .WithMany(u => u.AssignedSubtasks)
            .HasForeignKey(s => s.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Subtask → CreatedBy (restrict to avoid multiple cascade paths)
        modelBuilder.Entity<Subtask>()
            .HasOne(s => s.CreatedBy)
            .WithMany(u => u.CreatedSubtasks)          // add this nav in Users
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // ── TaskAssignment composite PK ───────────────────────────────────────
        modelBuilder.Entity<TaskAssignment>()
            .HasKey(ta => new { ta.TaskItemId, ta.UserId });

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.TaskItem)
            .WithMany(t => t.TaskAssignments)
            .HasForeignKey(ta => ta.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.User)
            .WithMany(u => u.TaskAssignments)
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Restrict);        // avoid multi-cascade on Users

        // ── Comment ──────────────────────────────────────────────────────────
        // Inside OnModelCreating:

        // ── TaskComment ───────────────────────────────────────────────────────
        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.TaskComments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskComment>()
            .HasOne(c => c.TaskItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── SubtaskComment ────────────────────────────────────────────────────
        modelBuilder.Entity<SubtaskComment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.SubtaskComments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SubtaskComment>()
            .HasOne(c => c.Subtask)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.SubtaskId)
            .OnDelete(DeleteBehavior.NoAction);    // Subtask already cascades from TaskItem     // Subtask already cascades from TaskItem

        // ── AuditLog ─────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.PerformedBy)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        // EntityType + EntityId are plain columns (no FK) — intentional
        // so logs survive even if the task/subtask row is deleted.

        // ── Schedule ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Organizer)
            .WithMany(u => u.OrganizedSchedules)
            .HasForeignKey(s => s.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.TaskItem)
            .WithMany(t => t.Schedules)
            .HasForeignKey(s => s.TaskItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ── ScheduleParticipant composite PK ─────────────────────────────────
        modelBuilder.Entity<ScheduleParticipant>()
            .HasKey(sp => new { sp.ScheduleId, sp.UserId });

        modelBuilder.Entity<ScheduleParticipant>()
            .HasOne(sp => sp.Schedule)
            .WithMany(s => s.Participants)
            .HasForeignKey(sp => sp.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ScheduleParticipant>()
            .HasOne(sp => sp.User)
            .WithMany(u => u.ScheduleParticipants)
            .HasForeignKey(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Restrict);        // avoid multi-cascade on Users
    }
}