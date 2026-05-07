using System.ComponentModel.DataAnnotations.Schema;
namespace PlumbingAIS.Backend.Models
{
    public class Contractor : DictionaryEntity
    {
        [NotMapped]
        public string CompanyName { get; set; } = string.Empty;

        public string Type { get; set; } = "Supplier";
        public string? ContactInfo { get; set; }
    }
}