using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}