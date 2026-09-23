using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBasedService;
using HACK26.MS.MyMoneyRules.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using HACK26.MS.MyMoneyRules.Data.Validations;
using System;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Host
{
    public class DistributedService : ProviderBasedService<IMyMoneyRulesServiceContract, ServiceImp>, IMyMoneyRulesServiceContract
    {
        private IMyMoneyRulesServiceContract _serviceContract; //This doesn't need to be injected, since the service will ALWAYS run the instance
        private readonly string _providerName;
        private readonly string _providerType;

        /// <summary>
        /// The new provider based service will require additional parameters during construction
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="providerName"></param>
        /// <param name="providerType"></param>
        public DistributedService(string friendlyName, string providerName, string providerType) : base(friendlyName, providerName, providerType)
        {
            _providerName = providerName;
            _providerType = providerType;
        }

        public void OnStart()
        {
            _serviceContract = new ServiceImp(_providerType, _providerName);

            // TODO: Add validators here.

            Alkami.Broker.ZeroMq.Setup.PublishUsingZeroMqLocally();
            Alkami.Broker.ZeroMq.Setup.SubscribeUsingZeroMqLocally(_serviceCancellationToken.Token);
            Alkami.Broker.App.Subscription.InitializeSubscriber();

            base.Start();
        }

        public void OnStop(TimeSpan fromSeconds)
        {
            base.Stop(fromSeconds);
        }

        /// <inheritdoc />
        public Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            return _serviceContract.GetSettingsAsync(request);
        }

        /// <inheritdoc />
        public Task<DecisionRuleResponse> GetDecisionRulesAsync(GetDecisionRulesRequest request)
        {
            return _serviceContract.GetDecisionRulesAsync(request);
        }

        /// <inheritdoc />
        public Task<DecisionRuleResponse> AddOrUpdateDecisionRulesAsync(AddOrUpdateDecisionRuleRequest request)
        {
            return _serviceContract.AddOrUpdateDecisionRulesAsync(request);
        }

        /// <inheritdoc />
        public Task<DecisionRuleResponse> DeleteDecisionRulesAsync(DeleteDecisionRulesRequest request)
        {
            return _serviceContract.DeleteDecisionRulesAsync(request);
        }

        /// <inheritdoc />
        public Task<FieldDefinitionResponse> GetFieldDefinitionsAsync(GetFieldDefinitionsRequest request)
        {
            return _serviceContract.GetFieldDefinitionsAsync(request);
        }

        /// <inheritdoc />
        public Task<TransactionEvaluationResponse> EvaluateTransactionAsync(EvaluateTransactionRequest request)
        {
            return _serviceContract.EvaluateTransactionAsync(request);
        }

        /// <inheritdoc />
        public Task<RuleEvaluationResponse> GetRuleEvaluationsAsync(GetRuleEvaluationsRequest request)
        {
            return _serviceContract.GetRuleEvaluationsAsync(request);
        }
    }
}