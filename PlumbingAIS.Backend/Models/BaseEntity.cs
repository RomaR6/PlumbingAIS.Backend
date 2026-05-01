using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}