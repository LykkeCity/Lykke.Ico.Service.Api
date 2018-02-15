using Lykke.AzureStorage.Tables;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    internal class CampaignInfoEntity : AzureTableEntity
    {
        [IgnoreProperty]
        public string Name
        {
            get => RowKey;
        }

        public string Value { get; set; }
    }
}
