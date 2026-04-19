namespace PlumbingAIS.Backend.DTOs
{
    public class ProductReadDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Material { get; set; }
        public string? Diameter { get; set; }
        public string? ThreadType { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }
}