namespace PlumbingAIS.Backend.Models
{
    public class ActionLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public User? User { get; set; }
    }
}