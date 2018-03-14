using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.IcoApi.Core.Domain.Campaign
{
    public interface ICampaignSettingsHistoryItem
    {
        string Username { get; }
        string Settings { get; }
        DateTime ChangedUtc { get; }
    }
}
