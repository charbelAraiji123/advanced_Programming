namespace Final_Project_Adv.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entity, int entityId, object? oldValue, object? newValue, int userId);
    }
}