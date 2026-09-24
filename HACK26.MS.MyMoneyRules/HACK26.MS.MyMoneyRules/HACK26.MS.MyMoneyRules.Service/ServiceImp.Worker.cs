using HACK26.MS.MyMoneyRules.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <summary>
    /// JSON store operations used by <see cref="RulesBackgroundWorker"/>
    /// </summary>
    public partial class ServiceImp
    {
        /// <summary>
        /// Transaction ids that have already been evaluated
        /// </summary>
        internal static HashSet<long> GetEvaluatedTransactionIds()
        {
            lock (StoreLock)
            {
                return new HashSet<long>(ReadTable<TransactionEvent>(TransactionEventsFile).Select(e => e.TransactionId));
            }
        }

        /// <summary>
        /// Update the status of action executions
        /// </summary>
        internal static void UpdateActionExecutionStatuses(IDictionary<int, string> statusByExecutionId)
        {
            if (statusByExecutionId == null || statusByExecutionId.Count == 0)
            {
                return;
            }

            lock (StoreLock)
            {
                var executions = ReadTable<ActionExecution>(ActionExecutionsFile);
                foreach (var execution in executions)
                {
                    if (statusByExecutionId.TryGetValue(execution.ActionExecutionId, out var status))
                    {
                        execution.Status = status;
                        execution.ExecutionDateUtc = DateTime.UtcNow;
                    }
                }

                WriteTable(ActionExecutionsFile, executions.Select(e => new { e.ActionExecutionId, e.EvaluationId, e.ActionId, e.Status, e.ExecutionDateUtc }));
            }
        }
    }
}
