using Autofac;
using AzureStorage;
using AzureStorage.Queue;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Ico.Core.Contracts.Emails;
using Lykke.Ico.Core.Services;
using Lykke.Service.IcoApi.AzureRepositories;
using Lykke.Service.IcoApi.AzureRepositories.Entities;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IcoApi.Modules
{
    class RepositoryModule : Module
    {
        private readonly IReloadingManager<IcoApiSettings> _settings;
        private readonly ILog _log;

        public RepositoryModule(IReloadingManager<IcoApiSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string IcoDataConnectionString(IcoApiSettings x) => x.Db.IcoDataConnString;
            var connectionStringManager = _settings.ConnectionString(IcoDataConnectionString);

            var investorTable = CreateTable<InvestorEntity>(connectionStringManager, "Investors");

            builder.RegisterInstance<IInvestorRepository>(new InvestorRepository(investorTable));

            builder.RegisterInstance<IEmailsQueuePublisher<InvestorConfirmation>>(
                new EmailsQueuePublisher<InvestorConfirmation>(connectionStringManager));
        }

        private INoSQLTableStorage<T> CreateTable<T>(IReloadingManager<string> connectionStringManager, string name)
            where T : TableEntity, new()
        {
            return AzureTableStorage<T>.Create(connectionStringManager, name, _log);
        }
    }
}
