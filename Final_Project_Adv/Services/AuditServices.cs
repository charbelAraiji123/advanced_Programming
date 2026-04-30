using Final_Project_Adv.Models;
using Final_Project_Adv.Infrastructure.Data;
using System.Text.Json;

namespace Final_Project_Adv.Services
{


    public class AuditService : IAuditService
    {
        private readonly AppDbContext context;

        public AuditService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task LogAsync(
            string action,
            string entityType,
            int entityId,
            object? oldValue,
            object? newValue,
            int performedById)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                PerformedById = performedById,
                PerformedAt = DateTime.UtcNow
            };

            context.AuditLog.Add(log);
            await context.SaveChangesAsync();
        }
    }
}