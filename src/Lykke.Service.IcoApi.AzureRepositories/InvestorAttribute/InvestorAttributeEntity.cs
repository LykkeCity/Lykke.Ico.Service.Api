using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Services.IcoApi.AzureRepositories
{
    internal class InvestorAttributeEntity : TableEntity
    {
        public string Email { get; set; }

        public static InvestorAttributeEntity Create(string email)
        {
            return new InvestorAttributeEntity
            {
                Email = email
            };
        }
    }
}
