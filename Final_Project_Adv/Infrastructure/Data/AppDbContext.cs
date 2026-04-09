using Final_Project_Adv.Domain.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Subtask → AssignedTo (optional)
        modelBuilder.Entity<Subtask>()
            .HasOne(s => s.AssignedTo)
            .WithMany(u => u.AssignedSubtasks)
            .HasForeignKey(s => s.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // TaskAssignment composite key
        modelBuilder.Entity<TaskAssignment>()
            .HasKey(t => new { t.TaskItemId, t.UserId }); 

        // TaskAssignment relationships
        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.TaskItem)
            .WithMany(t => t.TaskAssignments)
            .HasForeignKey(ta => ta.TaskItemId);

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.User)
            .WithMany(u => u.TaskAssignments)
            .HasForeignKey(ta => ta.UserId);

        
    }
}