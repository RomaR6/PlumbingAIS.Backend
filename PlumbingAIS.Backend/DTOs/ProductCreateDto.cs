using System.ComponentModel.DataAnnotations;

namespace PlumbingAIS.Backend.DTOs
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Артикул (SKU) обов'язковий")]
        [StringLength(50, ErrorMessage = "Артикул не може бути довшим за 50 символів")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Назва товару обов'язкова")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва має бути від 3 до 100 символів")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Ціна має бути більше нуля")]
        public decimal Price { get; set; }

        [Range(0, 1000, ErrorMessage = "Поріг залишку має бути від 0 до 1000")]
        public decimal MinThreshold { get; set; } = 5.0m;

        [StringLength(50)]
        public string? Material { get; set; }

        [StringLength(20)]
        public string? Diameter { get; set; }

        [StringLength(20)]
        public string? ThreadType { get; set; }

        [Required(ErrorMessage = "Категорія обов'язкова")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Бренд обов'язковий")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Одиниця виміру обов'язкова")]
        public int UnitId { get; set; }
    }
}