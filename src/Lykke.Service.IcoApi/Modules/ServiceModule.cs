using Autofac;
using Common.Log;
using Lykke.JobTriggers.Extenstions;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Core.Queues.Emails;
using Lykke.Service.IcoApi.Core.Queues.Transactions;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Services;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoExRate.Client;
using Lykke.Services.IcoApi.AzureRepositories;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoApi.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<Core.Settings.ServiceSettings.IcoApiSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<Core.Settings.ServiceSettings.IcoApiSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionStringManager = _settings.ConnectionString(x => x.Db.IcoDataConnString);

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

            builder.RegisterType<InvestorEmailRepository>()
                .As<IInvestorEmailRepository>()
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

            builder.RegisterType<PrivateInvestorRepository>()
                .As<IPrivateInvestorRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<PrivateInvestorAttributeRepository>()
                .As<IPrivateInvestorAttributeRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorService>()
                .As<IInvestorService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue))
                .SingleInstance();

            builder.RegisterType<AdminService>()
                .As<IAdminService>()
                .WithParameter("btcNetwork", _settings.CurrentValue.BtcNetwork)
                .WithParameter("ethNetwork", _settings.CurrentValue.EthNetwork)
                .SingleInstance();

            builder.RegisterType<BtcService>()
                .As<IBtcService>()
                .WithParameter("btcNetwork", _settings.CurrentValue.BtcNetwork)
                .WithParameter("testSecretKey", _settings.CurrentValue.BtcTestSecretKey)
                .SingleInstance();

            builder.RegisterType<EthService>()
                .As<IEthService>()
                .WithParameter("ethNetworkUrl", _settings.CurrentValue.EthUrl)
                .WithParameter("testSecretKey", _settings.CurrentValue.EthTestSecretKey)
                .SingleInstance();

            builder.RegisterType<FiatService>()
                .As<IFiatService>()
                .SingleInstance();

            builder.RegisterType<CampaignService>()
                .As<ICampaignService>()
                .SingleInstance();

            builder.RegisterType<UrlEncryptionService>()
                .As<IUrlEncryptionService>()
                .WithParameter("key", _settings.CurrentValue.KycServiceEncriptionKey)
                .WithParameter("iv", _settings.CurrentValue.KycServiceEncriptionIv)
                .SingleInstance();

            builder.RegisterType<KycService>()
                .As<IKycService>()
                .SingleInstance();

            builder.RegisterType<PrivateInvestorService>()
                .As<IPrivateInvestorService>()
                .SingleInstance();

            builder.RegisterType<TransactionService>()
                .As<ITransactionService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue))
                .SingleInstance();

            builder.RegisterType<QueuePublisher<InvestorConfirmationMessage>>()
                .As<IQueuePublisher<InvestorConfirmationMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<QueuePublisher<InvestorSummaryMessage>>()
                .As<IQueuePublisher<InvestorSummaryMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<QueuePublisher<InvestorNewTransactionMessage>>()
                .As<IQueuePublisher<InvestorNewTransactionMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager));

            builder.RegisterType<QueuePublisher<TransactionMessage>>()
                .As<IQueuePublisher<TransactionMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager))
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
