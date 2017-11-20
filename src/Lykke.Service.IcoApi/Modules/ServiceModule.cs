using Autofac;
using Common.Log;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
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

            builder.RegisterType<InvestorRepository>()
                .As<IInvestorRepository>()
                .WithParameter(TypedParameter.From(connectionStringManager))
                .SingleInstance();

            builder.RegisterType<InvestorAttributeRepository>()
                .As<IInvestorAttributeRepository>()
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
