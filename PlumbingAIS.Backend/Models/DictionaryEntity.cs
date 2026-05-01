using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.Models
{
    public abstract class DictionaryEntity : AuditEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}