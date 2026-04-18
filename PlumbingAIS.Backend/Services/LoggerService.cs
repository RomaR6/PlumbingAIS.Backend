namespace PlumbingAIS.Backend.Services
{
    
    public delegate void LogHandler(string action, int userId);

    public class LoggerService
    {
        private readonly IServiceProvider _serviceProvider;

        public LoggerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        
        public event LogHandler OnActionExecuted;

        public void LogAction(string action, int userId)
        {
            
            OnActionExecuted?.Invoke(action, userId);
        }
    }
}