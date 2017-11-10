using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.IcoApi.AzureRepositories;
using Lykke.Service.IcoApi.AzureRepositories.Entities;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

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

            var investorTable = CreateTable<InvestorEntity>(IcoDataConnectionString, "Investors");

            builder.RegisterInstance<IInvestorRepository>(new InvestorRepository(investorTable));
        }

        private INoSQLTableStorage<T> CreateTable<T>(Func<IcoApiSettings, string> connectionString, string name)
            where T : TableEntity, new()
        {
            return AzureTableStorage<T>.Create(_settings.ConnectionString(connectionString), name, _log);
        }
    }
}
