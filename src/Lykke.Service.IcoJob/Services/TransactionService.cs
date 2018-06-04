using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Rest;
using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoExRate.Client;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Emails;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Services.Extensions;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoExRate.Client.AutorestClient.Models;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoJob.Helpers;

namespace Lykke.Service.IcoJob.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ILog _log;
        private readonly IIcoExRateClient _exRateClient;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;
        private readonly IInvestorTransactionRepository _investorTransactionRepository;
        private readonly IInvestorTransactionRefundRepository _investorTransactionRefundRepository;
        private readonly IInvestorRefundRepository _investorRefundRepository;
        private readonly IInvestorRepository _investorRepository;
        private readonly IKycService _kycService;
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;

        public TransactionService(
            ILog log,
            IIcoExRateClient exRateClient, 
            IInvestorAttributeRepository investorAttributeRepository, 
            ICampaignInfoRepository campaignInfoRepository,
            ICampaignSettingsRepository campaignSettingsRepository,
            IInvestorTransactionRepository investorTransactionRepository,
            IInvestorTransactionRefundRepository investorTransactionRefundRepository,
            IInvestorRefundRepository investorRefundRepository,
            IInvestorRepository investorRepository,
            IKycService kycService,
            IIcoCommonServiceClient icoCommonServiceClient)
        {
            _log = log;
            _exRateClient = exRateClient;
            _investorAttributeRepository = investorAttributeRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
            _investorTransactionRepository = investorTransactionRepository;
            _investorTransactionRefundRepository = investorTransactionRefundRepository;
            _investorRefundRepository = investorRefundRepository;
            _investorRepository = investorRepository;
            _kycService = kycService;
            _icoCommonServiceClient = icoCommonServiceClient;
        }

        public async Task Process(TransactionMessage msg)
        {
            await _log.WriteInfoAsync(nameof(Process),
                $"msg: {msg.ToJson()}", $"New transaction");

            if (string.IsNullOrEmpty(msg.UniqueId))
            {
                throw new InvalidOperationException($"UniqueId can not be empty");
            }
            if (string.IsNullOrEmpty(msg.Email))
            {
                throw new InvalidOperationException($"Email can not be empty");
            }

            var txProcessed = await WasTxAlreadyProcessed(msg);
            if (txProcessed)
            {
                return;
            }

            var investor = await _investorRepository.GetAsync(msg.Email);
            if (investor == null)
            {
                throw new InvalidOperationException($"Investor with email {msg.Email} was not found");
            }

            var settings = await _campaignSettingsRepository.GetAsync();
            if (settings == null)
            {
                throw new InvalidOperationException($"Campaign settings was not found");
            }

            var txType = msg.GetTxType(investor);
            var smarcTokenInfo = await settings.GetSmarcTokenInfo(_campaignInfoRepository, msg.CreatedUtc);
            var logiTokenInfo = await settings.GetLogiTokenInfo(_campaignInfoRepository, msg.CreatedUtc);

            var validTx = await IsTxValid(msg, smarcTokenInfo, logiTokenInfo, txType);
            if (validTx)
            {
                var transaction = await SaveTransaction(msg, settings, smarcTokenInfo, logiTokenInfo, txType);

                await UpdateCampaignAmounts(transaction, settings);
                await UpdateInvestorAmounts(transaction);
                await UpdateLatestTransactions(transaction);
                await SendConfirmationEmail(transaction, msg.Link, settings);
            }
        }

        private async Task<bool> WasTxAlreadyProcessed(TransactionMessage msg)
        {
            var existingTransaction = await _investorTransactionRepository.GetAsync(msg.Email, msg.UniqueId);
            if (existingTransaction != null)
            {
                await _log.WriteInfoAsync(nameof(Process),
                    $"email: {msg.Email}, uniqueId: {msg.UniqueId}, existingTransaction: {existingTransaction.ToJson()}",
                    $"The transaction was already processed");

                return true;
            }

            var refundedTransaction = await _investorTransactionRefundRepository.GetAsync(msg.Email, msg.UniqueId);
            if (refundedTransaction != null)
            {
                await _log.WriteInfoAsync(nameof(Process),
                    $"email: {msg.Email}, uniqueId: {msg.UniqueId}, refundedTransaction: {refundedTransaction.ToJson()}",
                    $"The transaction was already refunded");

                return true;
            }

            return false;
        }

        private async Task<bool> IsTxValid(TransactionMessage msg, 
            TokenInfo smarcTokenInfo, 
            TokenInfo logiTokenInfo, 
            TxType txType)
        {
            if (txType == TxType.Smarc)
            {
                if (!string.IsNullOrEmpty(smarcTokenInfo.Error))
                {
                    await _log.WriteInfoAsync(nameof(Process),
                        $"msg: {msg}", smarcTokenInfo.Error);

                    await _investorRefundRepository.SaveAsync(msg.Email,
                        smarcTokenInfo.ErrorReason.Value,
                        msg.ToJson());

                    return false;
                }
            }

            if (txType == TxType.Logi)
            {
                if (!string.IsNullOrEmpty(logiTokenInfo.Error))
                {
                    await _log.WriteInfoAsync(nameof(Process),
                        $"msg: {msg}", logiTokenInfo.Error);

                    await _investorRefundRepository.SaveAsync(msg.Email,
                        logiTokenInfo.ErrorReason.Value,
                        msg.ToJson());

                    return false;
                }
            }

            if (txType == TxType.Smarc90Logi10)
            {
                if (!string.IsNullOrEmpty(smarcTokenInfo.Error))
                {
                    await _log.WriteInfoAsync(nameof(Process),
                        $"msg: {msg}", smarcTokenInfo.Error);

                    await _investorRefundRepository.SaveAsync(msg.Email,
                        smarcTokenInfo.ErrorReason.Value,
                        msg.ToJson());

                    return false;
                }

                if (!string.IsNullOrEmpty(logiTokenInfo.Error))
                {
                    await _log.WriteInfoAsync(nameof(Process),
                        $"msg: {msg}", logiTokenInfo.Error);

                    await _investorRefundRepository.SaveAsync(msg.Email,
                        logiTokenInfo.ErrorReason.Value,
                        msg.ToJson());

                    return false;
                }
            }

            return true;
        }

        private async Task<InvestorTransaction> SaveTransaction(TransactionMessage msg, ICampaignSettings settings,
            TokenInfo smarcTokenInfo, TokenInfo logiTokenInfo, TxType txType)
        {
            var exchangeRate = await GetExchangeRate(msg, settings);
            var amountUsd = msg.Amount * exchangeRate.ExchangeRate;

            var smartTokenContext = GetTokenContext(msg, settings, smarcTokenInfo, txType, amountUsd);
            var logiTokenContext = GetTokenContext(msg, settings, logiTokenInfo, txType, amountUsd);

            var tx = new InvestorTransaction
            {
                Email = msg.Email,
                UniqueId = msg.UniqueId,
                CreatedUtc = msg.CreatedUtc,
                Currency = msg.Currency,
                TransactionId = msg.TransactionId,
                BlockId = msg.BlockId,
                PayInAddress = msg.PayInAddress,
                Amount = msg.Amount,
                AmountUsd = amountUsd,
                Fee = msg.Fee,
                SmarcAmountToken = smartTokenContext.TokenAmount,
                SmarcAmountUsd = smartTokenContext.UsdAmount,
                SmarcTokenPriceUsd = smartTokenContext.TokenPriceUsd,
                SmarcTokenPriceContext = smartTokenContext.TxTokens.ToJson(),
                LogiAmountToken = logiTokenContext.TokenAmount,
                LogiAmountUsd = logiTokenContext.UsdAmount,
                LogiTokenPriceUsd = logiTokenContext.TokenPriceUsd,
                LogiTokenPriceContext = logiTokenContext.TxTokens.ToJson(),
                ExchangeRate = exchangeRate.ExchangeRate,
                ExchangeRateContext = exchangeRate.Context
            };

            await _log.WriteInfoAsync(nameof(SaveTransaction),
                $"tx: {tx.ToJson()}", $"Save transaction");

            await _investorTransactionRepository.SaveAsync(tx);

            return tx;
        }

        private TxTokenContext GetTokenContext(TransactionMessage msg,
            ICampaignSettings settings,
            TokenInfo tokenInfo,
            TxType txType,
            decimal amountUsd)
        {
            var txTokens = new List<TxToken>();

            if ((txType == TxType.Logi && tokenInfo.Name == Consts.SMARC) ||
                (txType == TxType.Smarc && tokenInfo.Name == Consts.LOGI))
            {
                return TxTokenContext.Create(txTokens);
            }

            if (txType == TxType.Smarc90Logi10)
            {
                if (tokenInfo.Name == Consts.SMARC)
                {
                    amountUsd = amountUsd * 0.9M;
                }
                if (tokenInfo.Name == Consts.LOGI)
                {
                    amountUsd = amountUsd * 0.1M;
                }
            }

            var tokenAmount = (amountUsd / tokenInfo.PriceUsd.Value).RoundDown(settings.RowndDownTokenDecimals);

            if (tokenAmount > tokenInfo.PhaseTokenAmountAvailable.Value &&
                (tokenInfo.Tier == CampaignTier.CrowdSale1stTier || tokenInfo.Tier == CampaignTier.CrowdSale2ndTier))
            {
                // below threshold
                txTokens.Add(new TxToken
                {
                    Name = tokenInfo.Name,
                    Amount = tokenInfo.PhaseTokenAmountAvailable.Value,
                    PriceUsd = tokenInfo.PriceUsd.Value,
                    Phase = Enum.GetName(typeof(CampaignTier), tokenInfo.Tier)
                });

                // above threshold
                var nextTierContext = GetNextTierTokenContext(tokenInfo, settings, amountUsd);

                txTokens.Add(new TxToken
                {
                    Name = tokenInfo.Name,
                    Amount = nextTierContext.Amount,
                    PriceUsd = nextTierContext.PriceUsd,
                    Phase = nextTierContext.Phase
                });

                return TxTokenContext.Create(txTokens);
            }

            txTokens.Add(new TxToken
            {
                Name = tokenInfo.Name,
                Amount = tokenAmount,
                PriceUsd = tokenInfo.PriceUsd.Value,
                Phase = Enum.GetName(typeof(CampaignTier), tokenInfo.Tier)
            });

            return TxTokenContext.Create(txTokens);
        }

        private (string Phase, decimal PriceUsd, decimal Amount) GetNextTierTokenContext(TokenInfo tokenInfo, 
            ICampaignSettings settings, decimal amountUsd)
        {
            var phase = "";
            var priceUsd = 0M;

            if (tokenInfo.Tier == CampaignTier.CrowdSale1stTier)
            {
                phase = nameof(CampaignTier.CrowdSale1stTier);
                priceUsd = tokenInfo.Name == Consts.SMARC ? settings.CrowdSale2ndTierSmarcPriceUsd : settings.CrowdSale2ndTierLogiPriceUsd;
            }

            if (tokenInfo.Tier == CampaignTier.CrowdSale2ndTier)
            {
                phase = nameof(CampaignTier.CrowdSale2ndTier);
                priceUsd = tokenInfo.Name == Consts.SMARC ? settings.CrowdSale3rdTierSmarcAmount : settings.CrowdSale3rdTierLogiAmount;
            }

            var amount = ((amountUsd - tokenInfo.PhaseAmountUsdAvailable.Value) / priceUsd).RoundDown(settings.RowndDownTokenDecimals);

            return (phase, priceUsd, amount);
        }
            

        private async Task<(decimal ExchangeRate, string Context)> GetExchangeRate(TransactionMessage msg, ICampaignSettings settings)
        {
            if (msg.Currency == CurrencyType.Fiat)
            {
                return (1, "");
            }

            var assetPair = msg.Currency == CurrencyType.Bitcoin ? Pair.BTCUSD : Pair.ETHUSD;
            var exchangeRate = await _exRateClient.GetAverageRate(assetPair, msg.CreatedUtc);
            if (exchangeRate == null)
            {
                throw new InvalidOperationException($"Exchange rate was not found");
            }
            if (exchangeRate.AverageRate == null || exchangeRate.AverageRate == 0)
            {
                throw new InvalidOperationException($"Exchange rate is not valid: {exchangeRate.ToJson()}");
            }

            var minExchangeRate = Convert.ToDecimal(exchangeRate.AverageRate);
            if (msg.Currency == CurrencyType.Ether &&
                settings.MinEthExchangeRate.HasValue &&
                minExchangeRate < settings.MinEthExchangeRate)
            {
                minExchangeRate = settings.MinEthExchangeRate.Value;
            }
            if (msg.Currency == CurrencyType.Bitcoin &&
                settings.MinBtcExchangeRate.HasValue &&
                minExchangeRate < settings.MinBtcExchangeRate)
            {
                minExchangeRate = settings.MinBtcExchangeRate.Value;
            }

            return (minExchangeRate, new { settings.MinEthExchangeRate, settings.MinBtcExchangeRate, ExchangeRate = exchangeRate }.ToJson());
        }

        private async Task SendConfirmationEmail(InvestorTransaction tx, string link, ICampaignSettings settings)
        {
            try
            {
                var investor = await _investorRepository.GetAsync(tx.Email);

                var message = new NewTransaction
                {
                    AuthToken = investor.ConfirmationToken.Value.ToString(),
                    InvestorAmountUsd = investor.AmountUsd.RoundDown(2),
                    InvestorAmountSmarcToken = investor.AmountSmarcToken.RoundDown(4),
                    InvestorAmountLogiToken = investor.AmountLogiToken.RoundDown(4),
                    TransactionAmount = tx.Amount,
                    TransactionAmountUsd = tx.AmountUsd.RoundDown(2),
                    TransactionAmountSmarcToken = tx.SmarcAmountToken.RoundDown(4),
                    TransactionAmountLogiToken = tx.LogiAmountToken.RoundDown(4),
                    TransactionFee = tx.Fee,
                    TransactionAsset = tx.Currency.ToAssetName(),
                    LinkTransactionDetails = link,
                    MinAmount = settings.MinInvestAmountUsd,
                    MoreInvestmentRequired = investor.AmountUsd < settings.MinInvestAmountUsd,
                    PayInAddress = tx.PayInAddress
                };

                if (settings.KycEnableRequestSending &&
                    investor.KycRequestedUtc == null &&
                    investor.AmountUsd >= settings.MinInvestAmountUsd)
                {
                    var kycId = await SaveInvestorKyc(investor.Email);
                    var kycLink = await _kycService.GetKycLink(tx.Email, kycId);

                    message.KycRequired = true;
                    message.KycLink = kycLink;
                }

                await _log.WriteInfoAsync(nameof(SendConfirmationEmail),
                    $"message: {message.ToJson()}",
                    $"Send transaction confirmation message to queue");

                await _icoCommonServiceClient.SendEmailAsync(new IcoCommon.Client.Models.EmailDataModel
                {
                    To = tx.Email,
                    TemplateId = "new-transaction",
                    CampaignId = Consts.CAMPAIGN_ID,
                    Data = message
                });
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(SendConfirmationEmail),
                    $"Tx: {tx.ToJson()}, TxLink: {link}",
                    ex);
            }
        }

        private async Task UpdateCampaignAmounts(InvestorTransaction tx, ICampaignSettings settings)
        {
            if (settings.IsPreSale(tx.CreatedUtc))
            {
                if (tx.Currency == CurrencyType.Bitcoin)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedBtc, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Ether)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedEth, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Fiat)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedFiat, tx.Amount);
                }

                await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedSmarcToken, tx.SmarcAmountToken);
                await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedLogiToken, tx.LogiAmountToken);
                await IncrementCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedUsd, tx.AmountUsd);
            }

            if (settings.IsCrowdSale(tx.CreatedUtc))
            {
                if (tx.Currency == CurrencyType.Bitcoin)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedBtc, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Ether)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedEth, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Fiat)
                {
                    await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedFiat, tx.Amount);
                }

                await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedSmarcToken, tx.SmarcAmountToken);
                await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedLogiToken, tx.LogiAmountToken);
                await IncrementCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedUsd, tx.AmountUsd);
            }
        }

        private async Task IncrementCampaignInfoParam(CampaignInfoType type, decimal value)
        {
            try
            {
                await _campaignInfoRepository.IncrementValue(type, value);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(IncrementCampaignInfoParam),
                    $"{Enum.GetName(typeof(CampaignInfoType), type)}: {value}",
                    ex);
            }
        }

        private async Task UpdateInvestorAmounts(InvestorTransaction tx)
        {
            try
            {
                await _investorRepository.IncrementAmount(tx.Email, tx.Currency, tx.Amount, tx.AmountUsd, 
                    tx.SmarcAmountToken, tx.LogiAmountToken);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(UpdateInvestorAmounts),
                    $"Tx: {tx.ToJson()}",
                    ex);
            }
        }

        private async Task UpdateLatestTransactions(InvestorTransaction tx)
        {
            try
            {
                await _campaignInfoRepository.SaveLatestTransactionsAsync(tx.Email, tx.UniqueId);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(UpdateLatestTransactions),
                    $"email: {tx.Email}, uniqueId: {tx.UniqueId}",
                    ex);
            }
        }

        private async Task<string> SaveInvestorKyc(string email)
        {
            try
            {
                var kycId = Guid.NewGuid().ToString();

                await _log.WriteInfoAsync(nameof(SaveInvestorKyc),
                    $"email: {email}, kycId: {kycId}",
                    $"Save KYC request info");

                await _investorRepository.SaveKycAsync(email, kycId);
                await _investorAttributeRepository.SaveAsync(InvestorAttributeType.KycId, email, kycId);

                return kycId;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(SaveInvestorKyc), 
                    $"Email: {email}", 
                    ex);

                throw;
            }
        }

        private class TxTokenContext
        {
            public static TxTokenContext Create(List<TxToken> txTokens)
            {
                return new TxTokenContext { TxTokens = txTokens };
            }

            public List<TxToken> TxTokens { get; set; }

            public decimal TokenAmount { get { return TxTokens.Sum(f => f.Amount); }  }

            public decimal UsdAmount { get { return TxTokens.Sum(f => f.AmountUsd); } }

            public decimal TokenPriceUsd
            {
                get
                {
                    if (TxTokens.Count == 1)
                    {
                        return TxTokens[0].PriceUsd;
                    }
                    if (TxTokens.Count > 1)
                    {
                        return UsdAmount / TokenAmount;
                    }

                    return 0M;
                }
            }
        }

        private class TxToken
        {
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public decimal PriceUsd { get; set; }
            public decimal AmountUsd { get { return Amount * PriceUsd; } }
            public string Phase { get; set; }
        }
    }
}
