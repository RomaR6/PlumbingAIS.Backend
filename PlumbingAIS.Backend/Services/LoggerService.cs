using PlumbingAIS.Backend.Interfaces;

namespace PlumbingAIS.Backend.Services
{
    public delegate void LogHandler(string action, int userId);

    public class LoggerService
    {
        public event LogHandler? OnActionExecuted;

        public void LogAction(string action, int userId)
        {
            
            OnActionExecuted?.Invoke(action, userId);
        }
    }
}