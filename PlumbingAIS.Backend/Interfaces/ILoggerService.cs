using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface ILoggerService
    {
        Task LogActionAsync(string action, int userId);
        void OnLowStockHandler(object sender, LowStockEventArgs e);
    }
}