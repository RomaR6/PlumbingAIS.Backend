namespace PlumbingAIS.Backend.Interfaces
{
    public interface ILoggerService
    {
        Task LogActionAsync(string action, int userId);
    }
}