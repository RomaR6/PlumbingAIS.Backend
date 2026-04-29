namespace PlumbingAIS.Backend.DTOs
{
    public class TransactionRequestDto
    {
        public string Type { get; set; } = string.Empty;
        public int? ContractorId { get; set; }
        public string? Description { get; set; }
        public List<TransactionItemRequestDto> Items { get; set; } = new List<TransactionItemRequestDto>();
    }

    public class TransactionItemRequestDto
    {
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}