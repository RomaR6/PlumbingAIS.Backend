namespace PlumbingAIS.Backend.Models
{
    public class Contractor : DictionaryEntity
    {
        public string Type { get; set; } = "Supplier";
        public string? ContactInfo { get; set; }
    }
}