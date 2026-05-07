using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public class Warehouse : DictionaryEntity
    {
        [NotMapped]
        public string Address { get; set; } = string.Empty;
    }
}