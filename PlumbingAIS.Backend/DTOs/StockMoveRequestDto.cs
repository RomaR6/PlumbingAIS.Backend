namespace PlumbingAIS.Backend.DTOs
{
    public class StockMoveRequestDto
    {
        public int ProductId { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public decimal Quantity { get; set; }
        public string? Description { get; set; }
    }
}