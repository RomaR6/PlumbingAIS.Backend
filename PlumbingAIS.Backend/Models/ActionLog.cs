namespace PlumbingAIS.Backend.Models
{
    public class ActionLog : AuditEntity
    {
        public int? UserId { get; set; }
        public string? Action { get; set; }
        public User? User { get; set; }
    }
}