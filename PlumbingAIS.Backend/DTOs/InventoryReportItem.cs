namespace PlumbingAIS.Backend.DTOs
{
    public class InventoryReportItem
    {
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal MinThreshold { get; set; }
    }
}