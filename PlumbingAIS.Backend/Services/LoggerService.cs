using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly AppDbContext _context;

        public LoggerService(AppDbContext context)
        {
            _context = context;
        }

        public void OnLowStockHandler(object sender, LowStockEventArgs e)
        {
            _ = LogActionAsync($"УВАГА: Критичний залишок товару {e.ProductName} (SKU: {e.SKU}). Залишилось: {e.CurrentQuantity}, Поріг: {e.Threshold}", 0);
        }

        public async Task LogActionAsync(string action, int userId)
        {
            var log = new ActionLog
            {
                Action = action,
                UserId = userId == 0 ? null : userId,
                CreatedAt = DateTime.Now
            };

            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}