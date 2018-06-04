﻿using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services.Extensions
{
    public static class CampaignSettingsExtenstions
    {
        public static decimal GetCrowdSaleSmarcAmount(this ICampaignSettings self)
        {
            return self.CrowdSale1stTierSmarcAmount + self.CrowdSale2ndTierSmarcAmount + self.CrowdSale3rdTierSmarcAmount;
        }

        public static decimal GetCrowdSaleLogiAmount(this ICampaignSettings self)
        {
            return self.CrowdSale1stTierLogiAmount + self.CrowdSale2ndTierLogiAmount + self.CrowdSale3rdTierLogiAmount;
        }

        public static bool IsPreSale(this ICampaignSettings self, DateTime txCreatedUtc)
        {
            if (txCreatedUtc >= self.CrowdSaleStartDateTimeUtc)
            {
                return false;
            }

            if (!self.PreSaleEndDateTimeUtc.HasValue)
            {
                return txCreatedUtc >= self.PreSaleStartDateTimeUtc;
            }

            return txCreatedUtc >= self.PreSaleStartDateTimeUtc && txCreatedUtc <= self.PreSaleEndDateTimeUtc.Value;
        }

        public static bool IsCrowdSale(this ICampaignSettings self, DateTime txCreatedUtc)
        {
            if (!self.CrowdSaleEndDateTimeUtc.HasValue)
            {
                return txCreatedUtc >= self.CrowdSaleStartDateTimeUtc;
            }

            return txCreatedUtc >= self.CrowdSaleStartDateTimeUtc && txCreatedUtc <= self.CrowdSaleEndDateTimeUtc.Value;
        }

        public static CampaignPhase? GetPhase(this ICampaignSettings self, DateTime nowUtc)
        {
            if (self.IsCrowdSale(nowUtc))
            {
                return CampaignPhase.CrowdSale;
            }

            if (self.IsPreSale(nowUtc))
            {
                return CampaignPhase.PreSale;
            }

            return null;
        }

        public static string GetPhaseString(this ICampaignSettings self, DateTime nowUtc)
        {
            var phase = self.GetPhase(nowUtc);

            return phase.HasValue ? Enum.GetName(typeof(CampaignPhase), phase) : null;
        }

        public static async Task<TokenInfo> GetSmarcTokenInfo(this ICampaignSettings self, 
            ICampaignInfoRepository campaignInfoRepository, 
            DateTime txDateTimeUtc)
        {
            if (self.IsPreSale(txDateTimeUtc))
            {
                var preSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountPreSaleInvestedSmarcToken);
                if (!Decimal.TryParse(preSaleTokensAmountStr, out var preSaleTokensAmount))
                {
                    preSaleTokensAmount = 0;
                }

                if (preSaleTokensAmount < self.PreSaleSmarcAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.SMARC,
                        PriceUsd = self.PreSaleSmarcPriceUsd,
                        Tier = CampaignTier.PreSale,
                        PhaseTokenAmount = preSaleTokensAmount,
                        PhaseTokenAmountTotal = self.PreSaleSmarcAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.SMARC,
                    ErrorReason = InvestorRefundReason.PreSaleTokensSoldOut,
                    Error = $"All presale SMARC tokens sold out. CurrentTokenAmount={preSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.PreSaleSmarcAmount}"
                };
            }

            if (self.IsCrowdSale(txDateTimeUtc))
            {
                var crowdSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountCrowdSaleInvestedSmarcToken);
                if (!Decimal.TryParse(crowdSaleTokensAmountStr, out var crowdSaleTokensAmount))
                {
                    crowdSaleTokensAmount = 0;
                }

                if (crowdSaleTokensAmount < self.CrowdSale1stTierSmarcAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.SMARC,
                        PriceUsd = self.CrowdSale1stTierSmarcPriceUsd,
                        Tier = CampaignTier.CrowdSale1stTier,
                        PhaseTokenAmount = crowdSaleTokensAmount,
                        PhaseTokenAmountTotal = self.CrowdSale1stTierSmarcAmount
                    };
                }

                var crowdSale2ndTierAmountTotal = self.CrowdSale1stTierSmarcAmount + self.CrowdSale2ndTierSmarcAmount;
                if (crowdSaleTokensAmount >= self.CrowdSale1stTierSmarcAmount &&
                    crowdSaleTokensAmount < crowdSale2ndTierAmountTotal)
                {
                    return new TokenInfo
                    {
                        Name = Consts.SMARC,
                        PriceUsd = self.CrowdSale2ndTierSmarcPriceUsd,
                        Tier = CampaignTier.CrowdSale2ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount - self.CrowdSale1stTierSmarcAmount,
                        PhaseTokenAmountTotal = self.CrowdSale2ndTierSmarcAmount
                    };
                }

                if (crowdSaleTokensAmount >= crowdSale2ndTierAmountTotal &&
                    crowdSaleTokensAmount < self.GetCrowdSaleSmarcAmount())
                {
                    return new TokenInfo
                    {
                        Name = Consts.SMARC,
                        PriceUsd = self.CrowdSale3rdTierSmarcPriceUsd,
                        Tier = CampaignTier.CrowdSale3ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount - crowdSale2ndTierAmountTotal,
                        PhaseTokenAmountTotal = self.CrowdSale3rdTierSmarcAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.SMARC,
                    ErrorReason = InvestorRefundReason.CrowdSaleTokensSoldOut,
                    Error = $"All crowdsale SMARC tokens sold out. CurrentTokenAmount={crowdSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.GetCrowdSaleSmarcAmount()}"
                };
            }

            return new TokenInfo
            {
                Name = Consts.SMARC,
                ErrorReason = InvestorRefundReason.OutOfDates,
                Error = "Out of campaign dates"
            };
        }

        public static async Task<TokenInfo> GetLogiTokenInfo(this ICampaignSettings self,
            ICampaignInfoRepository campaignInfoRepository,
            DateTime txDateTimeUtc)
        {
            if (self.IsPreSale(txDateTimeUtc))
            {
                var preSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountPreSaleInvestedLogiToken);
                if (!Decimal.TryParse(preSaleTokensAmountStr, out var preSaleTokensAmount))
                {
                    preSaleTokensAmount = 0;
                }

                if (preSaleTokensAmount < self.PreSaleLogiAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.LOGI,
                        PriceUsd = self.PreSaleLogiPriceUsd,
                        Tier = CampaignTier.PreSale,
                        PhaseTokenAmount = preSaleTokensAmount,
                        PhaseTokenAmountTotal = self.PreSaleLogiAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.LOGI,
                    ErrorReason = InvestorRefundReason.PreSaleTokensSoldOut,
                    Error = $"All presale LOGI tokens sold out. CurrentTokenAmount={preSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.PreSaleLogiAmount}"
                };
            }

            if (self.IsCrowdSale(txDateTimeUtc))
            {
                var crowdSaleTokensAmountStr = await campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountCrowdSaleInvestedLogiToken);
                if (!Decimal.TryParse(crowdSaleTokensAmountStr, out var crowdSaleTokensAmount))
                {
                    crowdSaleTokensAmount = 0;
                }

                if (crowdSaleTokensAmount < self.CrowdSale1stTierLogiAmount)
                {
                    return new TokenInfo
                    {
                        Name = Consts.LOGI,
                        PriceUsd = self.CrowdSale1stTierLogiPriceUsd,
                        Tier = CampaignTier.CrowdSale1stTier,
                        PhaseTokenAmount = crowdSaleTokensAmount,
                        PhaseTokenAmountTotal = self.CrowdSale1stTierLogiAmount
                    };
                }

                var crowdSale2ndTierAmountTotal = self.CrowdSale1stTierLogiAmount + self.CrowdSale2ndTierLogiAmount;
                if (crowdSaleTokensAmount >= self.CrowdSale1stTierLogiAmount &&
                    crowdSaleTokensAmount < crowdSale2ndTierAmountTotal)
                {
                    return new TokenInfo
                    {
                        Name = Consts.LOGI,
                        PriceUsd = self.CrowdSale2ndTierLogiPriceUsd,
                        Tier = CampaignTier.CrowdSale2ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount,
                        PhaseTokenAmountTotal = self.CrowdSale2ndTierLogiAmount
                    };
                }

                if (crowdSaleTokensAmount >= crowdSale2ndTierAmountTotal &&
                    crowdSaleTokensAmount < self.GetCrowdSaleLogiAmount())
                {
                    return new TokenInfo
                    {
                        Name = Consts.LOGI,
                        PriceUsd = self.CrowdSale3rdTierLogiPriceUsd,
                        Tier = CampaignTier.CrowdSale3ndTier,
                        PhaseTokenAmount = crowdSaleTokensAmount,
                        PhaseTokenAmountTotal = self.CrowdSale3rdTierLogiAmount
                    };
                }

                return new TokenInfo
                {
                    Name = Consts.LOGI,
                    ErrorReason = InvestorRefundReason.CrowdSaleTokensSoldOut,
                    Error = $"All crowdsale LOGI tokens sold out. CurrentTokenAmount={crowdSaleTokensAmount}. " +
                            $"AvailableTokenAmount={self.GetCrowdSaleLogiAmount()}"
                };
            }

            return new TokenInfo
            {
                Name = Consts.LOGI,
                ErrorReason = InvestorRefundReason.OutOfDates,
                Error = "Out of campaign dates"
            };
        }
    }
}
