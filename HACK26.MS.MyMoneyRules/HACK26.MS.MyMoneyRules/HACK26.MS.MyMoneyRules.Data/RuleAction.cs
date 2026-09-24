using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// An action performed when a rule matches. Stored in core.UserEngineActions.
    /// </summary>
    [Table("Actions")]
    public class RuleAction
    {
        /// <summary>
        /// Action identifier
        /// </summary>
        [Key] public int ActionId { get; set; }

        /// <summary>
        /// Identifier of the rule the action belongs to
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// Kind of action, such as SendNotification or TransferToSavings
        /// </summary>
        [MaxLength(100)] public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Action settings, usually JSON; for notifications {"channel":"push","message":"..."}
        /// </summary>
        [MaxLength(1000)] public string ActionValue { get; set; }

        /// <summary>
        /// The rule the action belongs to
        /// </summary>
        public DecisionRule Rule { get; set; }
    }
}
