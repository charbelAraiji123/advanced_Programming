using Final_Project_Adv.Models;
using Final_Project_Adv.Infrastructure.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Final_Project_Adv.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext context;

        // FIX: serializer options with cycle handling as a safety net,
        // in case any caller accidentally passes an object with circular references.
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };

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
                OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue, _jsonOptions) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue, _jsonOptions) : null,
                PerformedById = performedById,
                PerformedAt = DateTime.UtcNow
            };

            context.AuditLog.Add(log);
            await context.SaveChangesAsync();
        }
    }
}