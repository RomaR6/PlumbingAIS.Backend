using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}