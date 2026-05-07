using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public class Category : DictionaryEntity
    {
        [NotMapped]
        public string? Description { get; set; }
    }
}