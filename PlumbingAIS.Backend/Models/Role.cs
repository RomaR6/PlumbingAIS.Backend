using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public class Role : DictionaryEntity
    {
        [NotMapped]
        public string Description { get; set; } = string.Empty;
    }
}