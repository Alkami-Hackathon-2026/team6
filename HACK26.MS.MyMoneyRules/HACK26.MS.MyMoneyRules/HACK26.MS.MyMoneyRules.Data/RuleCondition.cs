using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A single comparison of a transaction field against a value. Stored in core.UserEngineConditions.
    /// </summary>
    [Table("Conditions")]
    public class RuleCondition
    {
        /// <summary>
        /// Condition identifier
        /// </summary>
        [Key] public int ConditionId { get; set; }

        /// <summary>
        /// Identifier of the condition group the condition belongs to
        /// </summary>
        public int ConditionGroupId { get; set; }

        /// <summary>
        /// Transaction field the condition checks, such as amount or merchantType
        /// </summary>
        [MaxLength(100)] public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Comparison operator, such as =, &gt;, contains or in
        /// </summary>
        [MaxLength(20)] public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// Value the field is compared against
        /// </summary>
        [MaxLength(500)] public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Comma-separated account ids the condition applies to. Null or empty applies to all accounts.
        /// </summary>
        [MaxLength(500)] public string AccountIds { get; set; }

        /// <summary>
        /// The condition group the condition belongs to
        /// </summary>
        public ConditionGroup ConditionGroup { get; set; }

        /// <summary>
        /// The parsed <see cref="AccountIds"/>; empty when the condition applies to all accounts
        /// </summary>
        public List<int> GetAccountIdList()
        {
            if (string.IsNullOrWhiteSpace(AccountIds))
            {
                return new List<int>();
            }

            return AccountIds.Split(',')
                .Select(v => int.TryParse(v.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? (int?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Set <see cref="AccountIds"/> from a list; null or empty applies to all accounts
        /// </summary>
        /// <param name="accountIds">Account ids the condition applies to</param>
        public void SetAccountIdList(IEnumerable<int> accountIds)
        {
            var ids = accountIds?.Distinct().ToList();
            AccountIds = ids == null || ids.Count == 0
                ? null
                : string.Join(",", ids.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// True when the condition applies to the given account
        /// </summary>
        /// <param name="accountId">Account id of the transaction being evaluated</param>
        public bool AppliesToAccount(int accountId)
        {
            var ids = GetAccountIdList();
            return ids.Count == 0 || ids.Contains(accountId);
        }
    }
}
