using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services.Extensions
{
    public static class CampaignSettingsExtenstions
    {
        public static decimal GetCrowdSaleAmount(this ICampaignSettings self)
        {
            return self.CrowdSale1stTierTokenAmount + self.CrowdSale2ndTierTokenAmount + self.CrowdSale3rdTierTokenAmount;
        }

        public static bool IsPreSale(this ICampaignSettings self, DateTime txCreatedUtc)
        {
            return txCreatedUtc >= self.PreSaleStartDateTimeUtc && txCreatedUtc <= self.PreSaleEndDateTimeUtc;
        }

        public static bool IsCrowdSale(this ICampaignSettings self, DateTime txCreatedUtc)
        {
            return txCreatedUtc >= self.CrowdSaleStartDateTimeUtc && txCreatedUtc <= self.CrowdSaleEndDateTimeUtc;
        }

        public static async Task<TokenInfo> GetTokenInfo(this ICampaignSettings self, 
            ICampaignInfoRepository campaignInfoRepository, 
            DateTime txDateTimeUtc)
        {
            if (self.IsPreSale(txDateTimeUtc))
            {
                var preSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountPreSaleInvestedToken);
                if (!Decimal.TryParse(preSaleTokensAmountStr, out var preSaleTokensAmount))
                {
                    preSaleTokensAmount = 0;
                }

                if (preSaleTokensAmount < self.PreSaleTokenAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.DEMO,
                        PriceUsd = self.PreSaleTokenPriceUsd,
                        Phase = CampaignPhase.PreSale,
                        PhaseTokenAmount = preSaleTokensAmount,
                        PhaseTokenAmountTotal = self.PreSaleTokenAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.DEMO,
                    ErrorReason = InvestorRefundReason.PreSaleTokensSoldOut,
                    Error = $"All presale {Consts.DEMO} tokens sold out. CurrentTokenAmount={preSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.PreSaleTokenAmount}"
                };
            }

            if (self.IsCrowdSale(txDateTimeUtc))
            {
                var crowdSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountCrowdSaleInvestedToken);
                if (!Decimal.TryParse(crowdSaleTokensAmountStr, out var crowdSaleTokensAmount))
                {
                    crowdSaleTokensAmount = 0;
                }

                if (crowdSaleTokensAmount < self.CrowdSale1stTierTokenAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.DEMO,
                        PriceUsd = self.CrowdSale1stTierTokenPriceUsd,
                        Phase = CampaignPhase.CrowdSale1stTier,
                        PhaseTokenAmount = crowdSaleTokensAmount,
                        PhaseTokenAmountTotal = self.CrowdSale1stTierTokenAmount
                    };
                }

                var crowdSale2ndTierAmountTotal = self.CrowdSale1stTierTokenAmount + self.CrowdSale2ndTierTokenAmount;
                if (crowdSaleTokensAmount >= self.CrowdSale1stTierTokenAmount &&
                    crowdSaleTokensAmount < crowdSale2ndTierAmountTotal)
                {
                    return new TokenInfo
                    {
                        Name = Consts.DEMO,
                        PriceUsd = self.CrowdSale2ndTierTokenPriceUsd,
                        Phase = CampaignPhase.CrowdSale2ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount - self.CrowdSale1stTierTokenAmount,
                        PhaseTokenAmountTotal = self.CrowdSale2ndTierTokenAmount
                    };
                }

                if (crowdSaleTokensAmount >= crowdSale2ndTierAmountTotal &&
                    crowdSaleTokensAmount < self.GetCrowdSaleAmount())
                {
                    return new TokenInfo
                    {
                        Name = Consts.DEMO,
                        PriceUsd = self.CrowdSale3rdTierTokenPriceUsd,
                        Phase = CampaignPhase.CrowdSale3ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount - crowdSale2ndTierAmountTotal,
                        PhaseTokenAmountTotal = self.CrowdSale3rdTierTokenAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.DEMO,
                    ErrorReason = InvestorRefundReason.CrowdSaleTokensSoldOut,
                    Error = $"All crowdsale {Consts.DEMO} tokens sold out. CurrentTokenAmount={crowdSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.GetCrowdSaleAmount()}"
                };
            }

            return new TokenInfo
            {
                Name = Consts.DEMO,
                ErrorReason = InvestorRefundReason.OutOfDates,
                Error = "Out of campaign dates"
            };
        }
    }
}
