namespace PlumbingAIS.Backend.Models
{
    public class Contractor : BaseEntity
    {
        public string Type { get; set; } = "Supplier"; 
        public string? ContactInfo { get; set; }
    }
}