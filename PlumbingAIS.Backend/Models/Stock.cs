namespace PlumbingAIS.Backend.Models
{
    public class Stock : BaseEntity
    {
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            private set => _quantity = value;
        }

        public void AddQuantity(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount must be positive");
            Quantity += amount;
        }

        public void RemoveQuantity(decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount must be positive");
            if (amount > Quantity) throw new InvalidOperationException("Insufficient stock");
            Quantity -= amount;
        }

        public Product? Product { get; set; }
        public Location? Location { get; set; }
    }
}