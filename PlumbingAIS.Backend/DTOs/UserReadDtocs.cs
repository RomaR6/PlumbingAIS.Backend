namespace PlumbingAIS.Backend.DTOs
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; 
    }
}