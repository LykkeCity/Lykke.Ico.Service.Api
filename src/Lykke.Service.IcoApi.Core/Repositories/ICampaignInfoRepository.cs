﻿using Lykke.Service.IcoApi.Core.Domain.Campaign;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Core.Repositories
{
    public interface ICampaignInfoRepository
    {
        Task<Dictionary<string, string>> GetAllAsync();
        Task<string> GetValueAsync(CampaignInfoType type);
        Task SaveValueAsync(CampaignInfoType type, string value);
        Task IncrementValue(CampaignInfoType type, double value);
        Task IncrementValue(CampaignInfoType type, int value);
        Task IncrementValue(CampaignInfoType type, decimal value);
        Task IncrementValue(CampaignInfoType type, ulong value);
        Task<List<(string Email, string UniqueId)>> GetLatestTransactionsAsync();
        Task SaveLatestTransactionsAsync(string email, string uniqueId);
    }
}