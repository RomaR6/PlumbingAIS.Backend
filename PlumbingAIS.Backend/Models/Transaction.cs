using System;
using System.Collections.Generic;

namespace PlumbingAIS.Backend.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; } = "In";
        public int UserId { get; set; }
        public int? ContractorId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Description { get; set; }

        public User? User { get; set; }
        public Contractor? Contractor { get; set; }

        public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    }
}