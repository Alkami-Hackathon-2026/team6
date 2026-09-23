using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class TransactionEvent
    {
        [Key] public int EventId { get; set; }
        public bool TransactionOccurred { get; set; }
        public int AccountId { get; set; }
        public long TransactionId { get; set; }
        [Column(TypeName = "decimal(19,4)")] public decimal Amount { get; set; }
        [Column(TypeName = "decimal(19,4)")] public decimal AvailableBalance { get; set; }
        [MaxLength(250)] public string MerchantName { get; set; }
        [MaxLength(100)] public string MerchantType { get; set; }
        [MaxLength(100)] public string TransactionType { get; set; } = string.Empty;
        public DateTime TransactionDateUtc { get; set; }
    }
}
