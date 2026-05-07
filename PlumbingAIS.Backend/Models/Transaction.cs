using System;
using System.Collections.Generic;

namespace PlumbingAIS.Backend.Models
{
    public class Transaction : BaseEntity
    {
        public string Type { get; set; } = "In";
        public int UserId { get; set; }
        public int? ContractorId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public User? User { get; set; }
        public Contractor? Contractor { get; set; }
        public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();

        
        public bool ValidateTransaction()
        {
            return !string.IsNullOrEmpty(Type) && TransactionItems != null;
        }

        public string GenerateDocumentNumber()
        {
            return $"TRX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}