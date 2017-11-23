using Autofac;
using Common.Log;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoApi.Services;
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
                
            builder.RegisterType<InvestorService>()
                .As<IInvestorService>()
                .SingleInstance();

            builder.RegisterType<BtcService>()
                .As<IBtcService>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.BtcNetwork))
                .SingleInstance();

            builder.RegisterType<EthService>()
                .As<IEthService>()
                .SingleInstance();

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
