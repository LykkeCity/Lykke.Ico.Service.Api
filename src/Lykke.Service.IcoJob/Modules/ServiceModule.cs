using Autofac;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.IcoApi.AzureRepositories.Auth;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Services;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoExRate.Client;
using Lykke.Service.IcoJob.Settings;
using Lykke.Services.IcoApi.AzureRepositories;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoJob.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<IcoJobSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<IcoJobSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionStringManager = _settings.ConnectionString(x => x.Db.IcoDataConnString);

            builder.RegisterInstance(_settings.CurrentValue)
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterIcoExRateClient(_settings.CurrentValue.IcoExRateServiceUrl, _log);

            builder.RegisterIcoCommonClient(_settings.CurrentValue.IcoCommonServiceUrl, _log);

            builder.RegisterType<InvestorRepository>()
                .As<IInvestorRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorAttributeRepository>()
                .As<IInvestorAttributeRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<AddressPoolRepository>()
                .As<IAddressPoolRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorHistoryRepository>()
                .As<IInvestorHistoryRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<AddressPoolHistoryRepository>()
                .As<IAddressPoolHistoryRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<CampaignInfoRepository>()
                .As<ICampaignInfoRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<CampaignSettingsRepository>()
                .As<ICampaignSettingsRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorTransactionRepository>()
                .As<IInvestorTransactionRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorRefundRepository>()
                .As<IInvestorRefundRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<KycService>()
                .As<IKycService>()
                .SingleInstance();

            builder.RegisterType<TransactionService>()
                .As<ITransactionService>()
                .SingleInstance();

            builder.RegisterType<QueuePublisher<TransactionMessage>>()
                .As<IQueuePublisher<TransactionMessage>>()
                .WithParameter("connectionStringManager", connectionStringManager)
                .WithParameter("queueName", $"{Consts.CAMPAIGN_ID.ToLower()}-transaction")
                .SingleInstance();

            builder.AddTriggers(
                pool =>
                {
                    pool.AddDefaultConnection(_settings.ConnectionString(x => x.AzureQueue.ConnectionString));
                });

            builder.RegisterInstance(_settings.CurrentValue);
        }
    }
}
