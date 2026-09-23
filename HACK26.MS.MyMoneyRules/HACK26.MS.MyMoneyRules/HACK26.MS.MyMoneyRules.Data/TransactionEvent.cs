using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A transaction that has been evaluated against rules. Stored in core.UserEngineTransactionEvents.
    /// </summary>
    public class TransactionEvent
    {
        /// <summary>
        /// Event identifier
        /// </summary>
        [Key] public int EventId { get; set; }

        /// <summary>
        /// Whether a transaction occurred; checked by the default rule trigger
        /// </summary>
        public bool TransactionOccurred { get; set; }

        /// <summary>
        /// Account the transaction posted to
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Identifier of the transaction in the source system
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        [Column(TypeName = "decimal(19,4)")] public decimal Amount { get; set; }

        /// <summary>
        /// Available balance on the account after the transaction
        /// </summary>
        [Column(TypeName = "decimal(19,4)")] public decimal AvailableBalance { get; set; }

        /// <summary>
        /// Merchant name
        /// </summary>
        [MaxLength(250)] public string MerchantName { get; set; }

        /// <summary>
        /// Merchant category, such as Electronics or Restaurant
        /// </summary>
        [MaxLength(100)] public string MerchantType { get; set; }

        /// <summary>
        /// Transaction type, such as Debit or Credit
        /// </summary>
        [MaxLength(100)] public string TransactionType { get; set; } = string.Empty;

        /// <summary>
        /// Date and time of the transaction in UTC
        /// </summary>
        public DateTime TransactionDateUtc { get; set; }
    }
}
