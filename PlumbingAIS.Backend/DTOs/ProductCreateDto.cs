namespace PlumbingAIS.Backend.DTOs
{
    public class ProductCreateDto
    {
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal MinThreshold { get; set; } = 5.0m;
        public string? Material { get; set; }
        public string? Diameter { get; set; }
        public string? ThreadType { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
    }
}