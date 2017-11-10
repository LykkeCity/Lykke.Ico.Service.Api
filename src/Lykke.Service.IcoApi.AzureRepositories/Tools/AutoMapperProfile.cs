using AutoMapper;
using Lykke.Service.IcoApi.AzureRepositories.Entities;
using Lykke.Service.IcoApi.Core.Domain.Ico;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.IcoApi.AzureRepositories.Tools
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            ForAllMaps((map, cfg) =>
            {
                if (map.DestinationType.IsSubclassOf(typeof(TableEntity)))
                {
                    cfg.ForMember("ETag", opt => opt.Ignore());
                    cfg.ForMember("PartitionKey", opt => opt.Ignore());
                    cfg.ForMember("RowKey", opt => opt.Ignore());
                    cfg.ForMember("Timestamp", opt => opt.Ignore());
                }
            });

            CreateMap<IInvestor, InvestorEntity>();
            CreateMap<InvestorEntity, Investor>();
        }
    }
}
