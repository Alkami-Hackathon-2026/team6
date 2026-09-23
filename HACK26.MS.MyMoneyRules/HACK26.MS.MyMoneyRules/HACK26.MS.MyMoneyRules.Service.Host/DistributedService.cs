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
    /// <summary>
    /// Hosts the MyMoneyRules service and forwards each operation to <see cref="ServiceImp"/>. Also runs the rules background worker.
    /// </summary>
    public class DistributedService : ProviderBasedService<IMyMoneyRulesServiceContract, ServiceImp>, IMyMoneyRulesServiceContract
    {
        private IMyMoneyRulesServiceContract _serviceContract;
        private RulesBackgroundWorker _backgroundWorker;
        private readonly string _providerName;
        private readonly string _providerType;

        /// <summary>
        /// The new provider based service will require additional parameters during construction
        /// </summary>
        /// <param name="friendlyName">Display name of the service</param>
        /// <param name="providerName">Unique provider name</param>
        /// <param name="providerType">Provider type designator</param>
        public DistributedService(string friendlyName, string providerName, string providerType) : base(friendlyName, providerName, providerType)
        {
            _providerName = providerName;
            _providerType = providerType;
        }

        /// <summary>
        /// Starts the service, the message broker subscriptions and the rules background worker
        /// </summary>
        public void OnStart()
        {
            _serviceContract = new ServiceImp(_providerType, _providerName);

            // TODO: Add validators here.
            EntityValidator.AddValidator(new AddOrUpdateSomethingRequestValidator());
            EntityValidator.AddValidator(new GetSomethingRequestValidator());
            EntityValidator.AddValidator(new CustomDataObjectFilterValidator());
            EntityValidator.AddValidator(new CustomDataObjectValidator());
            EntityValidator.AddValidator(new GeminiChatRequestValidator());

            Alkami.Broker.ZeroMq.Setup.PublishUsingZeroMqLocally();
            Alkami.Broker.ZeroMq.Setup.SubscribeUsingZeroMqLocally(_serviceCancellationToken.Token);
            Alkami.Broker.App.Subscription.InitializeSubscriber();

            base.Start();

            _backgroundWorker = new RulesBackgroundWorker(
                _serviceContract,
                new Integrations.NotConfiguredIntegrations(),
                new Integrations.NotConfiguredIntegrations(),
                new Integrations.NotConfiguredIntegrations(),
                TimeSpan.FromMinutes(1));
            _backgroundWorker.Start();
        }

        /// <summary>
        /// Stops the rules background worker and then the service
        /// </summary>
        /// <param name="fromSeconds">How long to wait for the service to stop</param>
        public void OnStop(TimeSpan fromSeconds)
        {
            _backgroundWorker?.Dispose();
            _backgroundWorker = null;

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

        public Task<GeminiStatusResponse> GetGeminiStatusAsync(GetGeminiStatusRequest request)
        {
            return _serviceContract.GetGeminiStatusAsync(request);
        }

        public Task<GeminiChatResponse> GenerateGeminiChatAsync(GeminiChatRequest request)
        {
            return _serviceContract.GenerateGeminiChatAsync(request);
        }
    }
}