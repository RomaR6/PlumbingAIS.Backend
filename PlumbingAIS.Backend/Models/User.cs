namespace PlumbingAIS.Backend.Models
{
    public class User : AuditEntity
    {
        public int RoleId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual Role? Role { get; set; }
        public string RoleName => Role?.Name ?? "Гість";
    }
}