using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public class Brand : DictionaryEntity
    {
        [NotMapped]
        public string CountryOfOrigin { get; set; } = string.Empty;
    }
}