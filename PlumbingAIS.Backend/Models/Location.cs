namespace PlumbingAIS.Backend.Models
{
    public class Location : DictionaryEntity
    {
        public int WarehouseId { get; set; }
        public string RowCode { get; set; } = string.Empty;
        public string RackCode { get; set; } = string.Empty;
        public string ShelfCode { get; set; } = string.Empty;
        public Warehouse? Warehouse { get; set; }
    }
}