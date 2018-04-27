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
using Lykke.Service.IcoApi.Core.Emails;

namespace Lykke.Service.IcoApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly IcoApiSettings _settings;
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorTransactionRepository _investorTransactionRepository;
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
            dictionary.Add("CampaignId", Consts.CampaignId);
            dictionary.Add("TokenName", Consts.TokenName);

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

                var itemHistory = await _addressPoolHistoryRepository.Get(id);
                if (itemHistory != null)
                {
                    list.Add((itemHistory.Id, itemHistory.BtcPublicKey, itemHistory.EthPublicKey));
                    continue;
                }

                list.Add((itemHistory.Id, "", ""));
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
                    Id = counter,
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

        class PublicKeysRow
        {
            public string btcPublic { get; set; }
            public string ethPublic { get; set; }
        }

        public string GenerateTransactionQueueSasUrl(DateTimeOffset expiryTime)
        {
            return _transactionQueuePublisher.GenerateSasUrl(expiryTime);
        }

        public async Task SendKycReminderEmails(IEnumerable<IInvestor> investors)
        {
            foreach (var investor in investors)
            {
                var message = new KycReminder
                {
                    AuthToken = investor.ConfirmationToken.Value.ToString()
                };

                await _log.WriteInfoAsync(nameof(AdminService), nameof(SendKycReminderEmails),
                    $"message={message.ToJson()}", "Send kyc reminder message to queue");

                await _icoCommonServiceClient.SendEmailAsync(new IcoCommon.Client.Models.EmailDataModel
                {
                    To = investor.Email,
                    TemplateId = Consts.Emails.KycReminder,
                    CampaignId = Consts.CampaignId,
                    Data = message
                });
            }
        }
    }
}
