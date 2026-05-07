using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public class Unit : DictionaryEntity
    {
        [NotMapped]
        public string ShortName { get; set; } = string.Empty;
    }
}