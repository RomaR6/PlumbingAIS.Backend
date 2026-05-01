namespace PlumbingAIS.Backend.Models
{
    public class LowStockEventArgs : EventArgs
    {
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal CurrentQuantity { get; set; }
        public decimal Threshold { get; set; }
    }
}