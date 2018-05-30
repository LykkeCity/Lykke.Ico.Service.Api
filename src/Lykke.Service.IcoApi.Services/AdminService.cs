using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Common;
using Common.Log;
using CsvHelper;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoApi.Services.Extensions;

namespace Lykke.Service.IcoApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly IcoApiSettings _settings;
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorTransactionRepository _investorTransactionRepository;
        private readonly IInvestorTransactionRefundRepository _investorTransactionRefundRepository;
        private readonly IInvestorTransactionHistoryRepository _investorTransactionHistoryRepository;
        private readonly IInvestorRefundRepository _investorRefundRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IInvestorHistoryRepository _investorHistoryRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IAddressPoolHistoryRepository _addressPoolHistoryRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;
        private readonly IQueuePublisher<TransactionMessage> _transactionQueuePublisher;
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;

        public AdminService(IcoApiSettings settings,
            ILog log,
            IInvestorRepository investorRepository,
            IInvestorTransactionRepository investorTransactionRepository,
            IInvestorTransactionRefundRepository investorTransactionRefundRepository,
            IInvestorTransactionHistoryRepository investorTransactionHistoryRepository,
            IInvestorRefundRepository investorRefundRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IInvestorHistoryRepository investorHistoryRepository,
            IAddressPoolHistoryRepository addressPoolHistoryRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IAddressPoolRepository addressPoolRepository,
            ICampaignSettingsRepository campaignSettingsRepository,
            IQueuePublisher<TransactionMessage> transactionQueuePublisher,
            IIcoCommonServiceClient icoCommonServiceClient)
        {
            _settings = settings;
            _log = log;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _investorHistoryRepository = investorHistoryRepository;
            _addressPoolHistoryRepository = addressPoolHistoryRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _addressPoolRepository = addressPoolRepository;
            _investorTransactionRepository = investorTransactionRepository;
            _investorTransactionRefundRepository = investorTransactionRefundRepository;
            _investorTransactionHistoryRepository = investorTransactionHistoryRepository;
            _investorRefundRepository = investorRefundRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
            _transactionQueuePublisher = transactionQueuePublisher;
            _icoCommonServiceClient = icoCommonServiceClient;
        }

        public async Task<Dictionary<string, string>> GetCampaignInfo()
        {
            var dictionary = await _campaignInfoRepository.GetAllAsync();

            if (dictionary.ContainsKey(nameof(CampaignInfoType.LatestTransactions)))
            {
                dictionary.Remove(nameof(CampaignInfoType.LatestTransactions));
            }                

            dictionary.Add("BctNetwork", _settings.BtcNetwork);
            dictionary.Add("EthNetwork", _settings.EthNetwork);
            dictionary.Add("CampaignId", Consts.CAMPAIGN_ID);

            return dictionary;
        }

        public async Task DeleteInvestorAsync(string email)
        {
            email = email.ToLowCase();

            var inverstor = await _investorRepository.GetAsync(email);
            if (inverstor != null)
            {
                await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.ConfirmationToken, inverstor.ConfirmationToken.ToString());
                await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.KycId, inverstor.KycRequestId);

                await _investorRepository.RemoveAsync(email);
            }
        }

        public async Task<IEnumerable<IInvestor>> GetAllInvestors()
        {
            return await _investorRepository.GetAllAsync();
        }

        public async Task<IEnumerable<IInvestorTransaction>> GetAllInvestorTransactions()
        {
            return await _investorTransactionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<IInvestorRefund>> GetAllInvestorFailedTransactions()
        {
            return await _investorRefundRepository.GetAllAsync();
        }

        public async Task<IEnumerable<IInvestorHistoryItem>> GetInvestorHistory(string email)
        {
            email = email.ToLowCase();

            return await _investorHistoryRepository.GetAsync(email);
        }

        public async Task DeleteInvestorAllDataAsync(string email)
        {
            email = email.ToLowCase();

            await _investorHistoryRepository.RemoveAsync(email);
            await _investorTransactionRepository.RemoveAsync(email);
            await _investorTransactionRefundRepository.RemoveAsync(email);
            await _investorRefundRepository.RemoveAsync(email);
            await _icoCommonServiceClient.DeleteSentEmailsAsync(email);
        }

        public async Task<IEnumerable<IInvestorTransaction>> GetInvestorTransactions(string email)
        {
            email = email.ToLowCase();

            return await _investorTransactionRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<IInvestorRefund>> GetInvestorRefunds(string email)
        {
            email = email.ToLowCase();

            return await _investorRefundRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<IInvestorRefund>> GetRefunds()
        {
            return await _investorRefundRepository.GetAllAsync();
        }

        public async Task<string> SendTransactionMessageAsync(string email, string payInAddress, 
            CurrencyType currency, DateTime? createdUtc, string uniqueId, decimal amount)
        {
            email = email.ToLowCase();

            var investor = await _investorRepository.GetAsync(email);
            if (investor == null)
            {
                throw new Exception($"Investor with email={email} was not found");
            }

            createdUtc = createdUtc ?? DateTime.UtcNow;
            uniqueId = string.IsNullOrEmpty(uniqueId) ? Guid.NewGuid().ToString() : uniqueId;

            var fee = 0M;
            if (currency == CurrencyType.Fiat)
            {
                fee = amount * 0.039M;
                amount = amount - fee;
            }

            var message = new TransactionMessage
            {
                Email = email,
                Amount = amount,
                PayInAddress = payInAddress,
                CreatedUtc = createdUtc.Value.ToUniversalTime(),
                Currency = currency,
                Fee = fee,
                TransactionId = $"fake_tx_{uniqueId}",
                UniqueId = $"fake_uid_{uniqueId}"
            };

            if (currency == CurrencyType.Bitcoin)
            {
                message.BlockId = $"fake_btc_block_{Guid.NewGuid()}";
                message.Link = $"http://test.valid.global/btc/{message.TransactionId}";
            }

            if (currency == CurrencyType.Ether)
            {
                message.BlockId = $"fake_eth_block_{Guid.NewGuid()}";
                message.Link = $"http://test.valid.global/eth/{message.TransactionId}";
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(SendTransactionMessageAsync), 
                $"message={message.ToJson()}", "Send transaction message to queque");

            await _transactionQueuePublisher.SendAsync(message);

            return message.UniqueId;
        }

        public async Task<IEnumerable<(int Id, string BtcPublicKey, string EthPublicKey)>> GetPublicKeys(int[] ids)
        {
            var list = new List<(int, string, string)>();

            foreach (var id in ids)
            {
                var item = await _addressPoolRepository.Get(id);
                if (item != null)
                {
                    list.Add((item.Id, item.BtcPublicKey, item.EthPublicKey));
                    continue;
                }

                list.Add((id, "", ""));
            }

            return list;
        }

        public async Task<IEnumerable<IInvestorTransaction>> GetLatestTransactions()
        {
            var list = await _campaignInfoRepository.GetLatestTransactionsAsync();
            if (list == null || !list.Any())
            {
                return Enumerable.Empty<IInvestorTransaction>();
            }

            var result = new List<IInvestorTransaction>();

            foreach (var item in list)
            {
                var tx = await _investorTransactionRepository.GetAsync(item.Email, item.UniqueId);
                if (tx != null)
                {
                    result.Add(tx);
                }
            }

            return result;
        }

        public async Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            email = email.ToLowCase();

            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress);
        }

        public async Task<int> ImportPublicKeys(StreamReader reader)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), 
                "", "Start of public keys import");

            var list = new List<IAddressPoolItem>();
            var csv = new CsvReader(reader);
            var counter = 0;

            var addressPoolTotalSizeStr = await _campaignInfoRepository.GetValueAsync(CampaignInfoType.AddressPoolTotalSize);
            if (!Int32.TryParse(addressPoolTotalSizeStr, out var addressPoolTotalSize))
            {
                addressPoolTotalSize = 0;
            }

            csv.Configuration.Delimiter = ";";
            csv.Configuration.Encoding = Encoding.ASCII;
            csv.Configuration.HasHeaderRecord = true;

            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                var record = csv.GetRecord<PublicKeysRow>();
                counter++;

                if (counter % 500 == 0)
                {
                    await _addressPoolRepository.AddBatchAsync(list);

                    list = new List<IAddressPoolItem>();
                }

                list.Add(new AddressPoolItem
                {
                    Id = addressPoolTotalSize + counter,
                    BtcPublicKey = record.btcPublic,
                    EthPublicKey = record.ethPublic
                });

                if (counter % 10000 == 0)
                {
                    await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), 
                        $"counter={counter}", "Importing progress");
                }
            }

            if (list.Count > 0)
            {
                await _addressPoolRepository.AddBatchAsync(list);
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), 
                "", "{counter} imported keys");
            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), 
                "", "End of public keys import");

            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolTotalSize, counter);
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolCurrentSize, counter);

            return counter;
        }

        private class PublicKeysRow
        {
            public string btcPublic { get; set; }
            public string ethPublic { get; set; }
        }

        public string GenerateTransactionQueueSasUrl(DateTime? expiryTime = null)
        {
            return _transactionQueuePublisher.GenerateSasUrl(expiryTime);
        }

        public async Task<string> RefundTransaction(string email, string uniqueId)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(RefundTransaction),
                new { email, uniqueId }.ToJson(), "Start to refund transaction");

            var tx = await _investorTransactionRepository.GetAsync(email, uniqueId);
            if (tx == null)
            {
                throw new ArgumentException($"Transaction with id={uniqueId} was not found for investor with {email}");
            }

            var settings = await _campaignSettingsRepository.GetAsync();
            if (settings == null)
            {
                throw new InvalidOperationException($"Campaign settings was not found");
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(RefundTransaction),
                tx.ToJson(), "Remove from InvestorTransactions table");
            await _investorTransactionRepository.RemoveAsync(email, uniqueId);

            await _log.WriteInfoAsync(nameof(AdminService), nameof(RefundTransaction),
                tx.ToJson(), "Save to InvestorTransactionRefunds table");
            await _investorTransactionRefundRepository.SaveAsync(email, uniqueId, tx.ToJson());

            await _log.WriteInfoAsync(nameof(AdminService), nameof(RefundTransaction),
                "", "Decrease campaign amounts");
            await DecreaseCampaignAmounts(tx, settings);

            await _log.WriteInfoAsync(nameof(AdminService), nameof(RefundTransaction),
                new { tx.Email, tx.Currency, tx.Amount, tx.AmountUsd, tx.SmarcAmountToken, tx.LogiAmountToken }.ToJson(),
                $"Decrease investor amounts");
            await _investorRepository.IncrementAmount(tx.Email, tx.Currency, -tx.Amount, -tx.AmountUsd,
                    -tx.SmarcAmountToken, -tx.LogiAmountToken);

            return "";
        }

        private async Task DecreaseCampaignAmounts(IInvestorTransaction tx, ICampaignSettings settings)
        {
            if (settings.IsPreSale(tx.CreatedUtc))
            {
                if (tx.Currency == CurrencyType.Bitcoin)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedBtc, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Ether)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedEth, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Fiat)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedFiat, tx.Amount);
                }

                await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedSmarcToken, tx.SmarcAmountToken);
                await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedLogiToken, tx.LogiAmountToken);
                await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedUsd, tx.AmountUsd);
            }
            if (settings.IsCrowdSale(tx.CreatedUtc))
            {
                if (tx.Currency == CurrencyType.Bitcoin)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedBtc, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Ether)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedEth, tx.Amount);
                }
                if (tx.Currency == CurrencyType.Fiat)
                {
                    await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedFiat, tx.Amount);
                }

                await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedSmarcToken, tx.SmarcAmountToken);
                await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedLogiToken, tx.LogiAmountToken);
                await DecreaseCampaignInfoParam(CampaignInfoType.AmountCrowdSaleInvestedUsd, tx.AmountUsd);
            }
        }

        private async Task DecreaseCampaignInfoParam(CampaignInfoType type, decimal value)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(DecreaseCampaignInfoParam),
                new { type = Enum.GetName(typeof(CampaignInfoType), type), value = value}.ToJson(), 
                $"Decrease CampaignInfo value");

            await _campaignInfoRepository.IncrementValue(type, -value);
        }

        public async Task<decimal> FixTransactionsSmarc90Logi10(bool saveChanges)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixTransactionsSmarc90Logi10),
                "", "Start to fix Smarc90Logi10 transactions");

            var settings = await _campaignSettingsRepository.GetAsync();
            if (settings == null)
            {
                throw new InvalidOperationException($"Campaign settings was not found");
            }

            var txs = await _investorTransactionRepository.GetAllAsync();

            var txsSmarc90Logi10 = txs.Where(f => f.SmarcAmountUsd > 0 && f.LogiAmountUsd > 0);
            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixTransactionsSmarc90Logi10),
                $"txsSmarc90Logi10.Count={txsSmarc90Logi10.Count()}", "Smarc90Logi10 txs number");

            var txsSmarc90Logi10Failed = txsSmarc90Logi10.Where(f => (f.LogiAmountUsd / f.AmountUsd) > 0.2M);
            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixTransactionsSmarc90Logi10),
                $"txsSmarc90Logi10Failed.Count={txsSmarc90Logi10Failed.Count()}", "Failed Smarc90Logi10 txs number");

            var diffTxLogiAmountTokenTotal = 0M;

            foreach (var txSmarc90Logi10Failed in txsSmarc90Logi10Failed)
            {
                var diffTxLogiAmountToken = await FixFailedSmarc90Logi10Transation(txSmarc90Logi10Failed, settings, saveChanges);

                diffTxLogiAmountTokenTotal += diffTxLogiAmountToken;
            }

            return diffTxLogiAmountTokenTotal;
        }

        private async Task<decimal> FixFailedSmarc90Logi10Transation(IInvestorTransaction txSmarc90Logi10Failed,
            ICampaignSettings settings, 
            bool saveChanges)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixFailedSmarc90Logi10Transation),
                txSmarc90Logi10Failed.ToJson(), "Transaction to fix");

            if (saveChanges)
            {
                await _investorTransactionHistoryRepository.SaveAsync(txSmarc90Logi10Failed.Email,
                    "FixFailedSmarc90Logi10Transation",
                    txSmarc90Logi10Failed.ToJson());
            }

            var oldTxLogiAmountUsd = txSmarc90Logi10Failed.LogiAmountUsd;
            var oldTxLogiAmountToken = txSmarc90Logi10Failed.LogiAmountToken;

            var newTxLogiAmountUsd = txSmarc90Logi10Failed.AmountUsd * 0.1M;
            var newTxLogiAmountToken = (newTxLogiAmountUsd / txSmarc90Logi10Failed.LogiTokenPriceUsd).RoundDown(settings.RowndDownTokenDecimals);

            newTxLogiAmountUsd = newTxLogiAmountToken * txSmarc90Logi10Failed.LogiTokenPriceUsd;

            var diffTxLogiAmountToken = oldTxLogiAmountToken - newTxLogiAmountToken;

            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixFailedSmarc90Logi10Transation),
                new { oldTxLogiAmountUsd, oldTxLogiAmountToken, newTxLogiAmountUsd, newTxLogiAmountToken, diffTxLogiAmountToken }.ToJson(), 
                "Data to fix");

            var txSmarc90Logi10 = await _investorTransactionRepository.GetAsync(txSmarc90Logi10Failed.Email, txSmarc90Logi10Failed.UniqueId);
            txSmarc90Logi10.LogiAmountUsd = newTxLogiAmountUsd;
            txSmarc90Logi10.LogiAmountToken = newTxLogiAmountToken;
            txSmarc90Logi10.LogiTokenPriceContext = new object[] {
                new
                {
                    Name = "LOGI",
                    Amount = newTxLogiAmountToken,
                    PriceUsd = txSmarc90Logi10.LogiTokenPriceUsd,
                    AmountUsd = newTxLogiAmountUsd,
                    Phase = "PreSale"
                }
            }.ToJson();

            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixFailedSmarc90Logi10Transation),
                txSmarc90Logi10.ToJson(), "Update transaction");
            if (saveChanges)
            {
                await _investorTransactionRepository.SaveAsync(txSmarc90Logi10);
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixFailedSmarc90Logi10Transation),
                new
                {
                    txSmarc90Logi10.Email, txSmarc90Logi10.Currency, Amount = 0M,
                    AmountUsd = 0M, SmarcAmountToken = 0M, LogiAmountToken = -diffTxLogiAmountToken
                }.ToJson(),
                $"Decrease investor amounts");
            if (saveChanges)
            {
                await _investorRepository.IncrementAmount(txSmarc90Logi10.Email, txSmarc90Logi10.Currency, 
                    0, 0, 0, -diffTxLogiAmountToken);
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(FixFailedSmarc90Logi10Transation),
                new { diffTxLogiAmountToken }.ToJson(),
                "Decrease campaign amounts");
            if (saveChanges)
            {
                await DecreaseCampaignInfoParam(CampaignInfoType.AmountPreSaleInvestedLogiToken, diffTxLogiAmountToken);
            }

            return diffTxLogiAmountToken;
        }
    }
}
