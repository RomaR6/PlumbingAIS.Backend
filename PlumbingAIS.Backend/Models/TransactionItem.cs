namespace PlumbingAIS.Backend.Models
{
    public class TransactionItem
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAtTime { get; set; }
        public Transaction? Transaction { get; set; }
        public Product? Product { get; set; }
    }
}