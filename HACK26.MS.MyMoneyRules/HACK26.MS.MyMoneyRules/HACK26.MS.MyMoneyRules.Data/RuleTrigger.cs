using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A check that must pass before a rule's conditions are evaluated. Stored in core.UserEngineTriggers.
    /// </summary>
    [Table("Triggers")]
    public class RuleTrigger
    {
        /// <summary>
        /// Trigger identifier
        /// </summary>
        [Key] public int TriggerId { get; set; }

        /// <summary>
        /// Identifier of the rule the trigger belongs to
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// Transaction field the trigger checks. Defaults to transactionOccurred.
        /// </summary>
        [MaxLength(100)] public string FieldName { get; set; } = "transactionOccurred";

        /// <summary>
        /// Comparison operator, such as = or &gt;
        /// </summary>
        [MaxLength(20)] public string Operator { get; set; } = "=";

        /// <summary>
        /// Value the field is compared against
        /// </summary>
        [MaxLength(500)] public string Value { get; set; } = "true";

        /// <summary>
        /// The rule the trigger belongs to
        /// </summary>
        public DecisionRule Rule { get; set; }
    }
}
