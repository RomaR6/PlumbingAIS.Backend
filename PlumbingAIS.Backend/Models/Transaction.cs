using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlumbingAIS.Backend.Models
{
    public enum TransactionType
    {
        In,
        Out,
        Move
    }

    public class Transaction : BaseEntity
    {
        public string Type { get; set; } = "In";

        [NotMapped]
        public TransactionType EnumType
        {
            get => Enum.TryParse<TransactionType>(Type, true, out var result) ? result : TransactionType.In;
            set => Type = value.ToString();
        }

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
            return TransactionItems != null && !string.IsNullOrEmpty(Type);
        }

        public string GenerateDocumentNumber()
        {
            return $"TRX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}