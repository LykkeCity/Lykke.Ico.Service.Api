using Autofac;
using Common.Log;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.AddressPoolHistory;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.CryptoInvestment;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoApi.Services;
using Lykke.Service.IcoExRate.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.IcoApi.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<IcoApiSettings> _settings;
        private readonly ILog _log;

        public ServiceModule(IReloadingManager<IcoApiSettings> settings, ILog log)
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

            builder.RegisterType<EmailHistoryRepository>()
                .As<IEmailHistoryRepository>()
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

            builder.RegisterType<CryptoInvestmentRepository>()
                .As<ICryptoInvestmentRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorService>()
                .As<IInvestorService>()
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
                .WithParameter("ethNetwork", _settings.CurrentValue.EthNetwork)
                .WithParameter("testSecretKey", _settings.CurrentValue.EthTestSecretKey)
                .SingleInstance();

            builder.RegisterType<QueuePublisher<InvestorConfirmationMessage>>()
                .As<IQueuePublisher<InvestorConfirmationMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<QueuePublisher<InvestorSummaryMessage>>()
                .As<IQueuePublisher<InvestorSummaryMessage>>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();
        }
    }
}
