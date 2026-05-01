namespace PlumbingAIS.Backend.Models
{
    public abstract class AuditEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}