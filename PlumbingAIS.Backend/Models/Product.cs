namespace PlumbingAIS.Backend.Models
{
    public class Product : BaseEntity
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? UnitId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal MinThreshold { get; set; } = 5.0m;
        public string? Material { get; set; }
        public string? Diameter { get; set; }
        public string? ThreadType { get; set; }

        public Category? Category { get; set; }
        public Brand? Brand { get; set; }
        public Unit? Unit { get; set; }
    }
}