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

        public async Task LogActionAsync(string action, int userId)
        {
            var log = new ActionLog
            {
                Action = action,
                UserId = userId,
                Timestamp = DateTime.Now
            };

            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}