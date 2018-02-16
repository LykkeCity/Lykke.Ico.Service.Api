using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Common;
using Common.Log;
using CsvHelper;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.AddressPoolHistory;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorTransaction;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Queues.Transactions;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Repositories.InvestorRefund;
using System.Linq;
using Newtonsoft.Json;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;

namespace Lykke.Service.IcoApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorTransactionRepository _investorTransactionRepository;
        private readonly IInvestorRefundRepository _investorRefundRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IInvestorEmailRepository _investorEmailRepositoryy;
        private readonly IInvestorHistoryRepository _investorHistoryRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IAddressPoolHistoryRepository _addressPoolHistoryRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly ICampaignSettingsRepository _campaignSettingsRepository;
        private readonly IQueuePublisher<TransactionMessage> _transactionQueuePublisher;
        private readonly IQueuePublisher<InvestorKycReminderMessage> _investorKycReminderPublisher;
        private readonly IcoApiSettings _icoApiSettings;

        public AdminService(ILog log,
            IInvestorRepository investorRepository,
            IInvestorTransactionRepository investorTransactionRepository,
            IInvestorRefundRepository investorRefundRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IInvestorEmailRepository emailHistoryRepository,
            IInvestorHistoryRepository investorHistoryRepository,
            IAddressPoolHistoryRepository addressPoolHistoryRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IAddressPoolRepository addressPoolRepository,
            ICampaignSettingsRepository campaignSettingsRepository,
            IQueuePublisher<TransactionMessage> transactionQueuePublisher,
            IQueuePublisher<InvestorKycReminderMessage> investorKycReminderPublisher,
            IcoApiSettings icoApiSettings)
        {
            _log = log;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _investorHistoryRepository = investorHistoryRepository;
            _investorEmailRepositoryy = emailHistoryRepository;
            _addressPoolHistoryRepository = addressPoolHistoryRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _addressPoolRepository = addressPoolRepository;
            _investorTransactionRepository = investorTransactionRepository;
            _investorRefundRepository = investorRefundRepository;
            _campaignSettingsRepository = campaignSettingsRepository;
            _transactionQueuePublisher = transactionQueuePublisher;
            _investorKycReminderPublisher = investorKycReminderPublisher;
            _icoApiSettings = icoApiSettings;
        }

        public async Task<Dictionary<string, string>> GetCampaignInfo()
        {
            var dictionary = await _campaignInfoRepository.GetAllAsync();

            if (dictionary.ContainsKey(nameof(CampaignInfoType.LatestTransactions)))
            {
                dictionary.Remove(nameof(CampaignInfoType.LatestTransactions));
            }                

            dictionary.Add("BctNetwork", _icoApiSettings.BtcNetwork);
            dictionary.Add("EthNetwork", _icoApiSettings.EthNetwork);

            return dictionary;
        }

        public async Task DeleteInvestorAsync(string email)
        {
            email = email.ToLowCase();

            var inverstor = await _investorRepository.GetAsync(email);
            if (inverstor != null)
            {
                if (inverstor.ConfirmationToken.HasValue)
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.ConfirmationToken, inverstor.ConfirmationToken.ToString());
                }
                if (!string.IsNullOrEmpty(inverstor.PayInBtcAddress))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.PayInBtcAddress, inverstor.PayInBtcPublicKey);
                }
                if (!string.IsNullOrEmpty(inverstor.PayInEthAddress))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.PayInEthAddress, inverstor.PayInEthPublicKey);
                }

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

            await _investorEmailRepositoryy.RemoveAsync(email);
            await _investorHistoryRepository.RemoveAsync(email);
            await _investorTransactionRepository.RemoveAsync(email);
            await _investorRefundRepository.RemoveAsync(email);
        }

        public async Task<IEnumerable<IInvestorEmail>> GetInvestorEmails(string email)
        {
            email = email.ToLowCase();

            return await _investorEmailRepositoryy.GetAsync(email);
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

        public async Task ResendRefunds()
        {
            var refunds = await _investorRefundRepository.GetAllAsync();
            if (!refunds.Any())
            {
                await _log.WriteInfoAsync(nameof(AdminService), nameof(ResendRefunds),
                    "There are not failed txs to resend");

                return;
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(ResendRefunds),
                    $"There are {refunds.Count()} failed txs to resend");

            refunds = refunds.OrderBy(f => f.CreatedUtc);

            foreach (var refund in refunds)
            {
                var message = JsonConvert.DeserializeObject<TransactionMessage>(refund.MessageJson);

                await _log.WriteInfoAsync(nameof(AdminService), nameof(ResendRefunds),
                    $"message={message.ToJson()}", "Send failed tx to queue");

                await _transactionQueuePublisher.SendAsync(message);
            }
        }

        public async Task<string> SendTransactionMessageAsync(string email, CurrencyType currency, 
            DateTime? createdUtc, string uniqueId, decimal amount)
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
                CreatedUtc = createdUtc.Value.ToUniversalTime(),
                Currency = currency,
                Fee = fee,
                TransactionId = $"fake_tx_{uniqueId}",
                UniqueId = $"fake_uid_{uniqueId}"
            };

            if (currency == CurrencyType.Bitcoin)
            {
                message.BlockId = $"fake_btc_block_{Guid.NewGuid()}";
                message.PayInAddress = investor.PayInBtcAddress;
                message.Link = $"http://test.valid.global/btc/{message.TransactionId}";
            }

            if (currency == CurrencyType.Ether)
            {
                message.BlockId = $"fake_eth_block_{Guid.NewGuid()}";
                message.PayInAddress = investor.PayInEthAddress;
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

        public async Task SendKycReminderEmails(IEnumerable<IInvestor> investors)
        {
            foreach (var investor in investors)
            {
                var message = new InvestorKycReminderMessage
                {
                    EmailTo = investor.Email,
                    LinkToSummaryPage = _icoApiSettings.SiteSummaryPageUrl.Replace("{token}", investor.ConfirmationToken.Value.ToString())
                };

                await _log.WriteInfoAsync(nameof(AdminService), nameof(SendKycReminderEmails),
                    $"message={message.ToJson()}", "Send kyc reminder message to queue");

                await _investorKycReminderPublisher.SendAsync(message);
            }
        }

        public async Task UpdateInvestorAsync(string email, string tokenAddress, string refundEthAddress, string refundBtcAddress)
        {
            email = email.ToLowCase();

            await _investorRepository.SaveAddressesAsync(email, tokenAddress, refundEthAddress, refundBtcAddress);
        }

        public async Task UpdateInvestorKycAsync(IInvestor investor, bool? kycPassed)
        {
            if (string.IsNullOrEmpty(investor.KycRequestId))
            {
                var kycId = Guid.NewGuid().ToString();

                await _investorRepository.SaveKycAsync(investor.Email, kycId);
                await _investorAttributeRepository.SaveAsync(InvestorAttributeType.KycId, investor.Email, kycId);
            }

            await _investorRepository.SaveKycResultAsync(investor.Email, kycPassed, true);
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

        private class PublicKeysRow
        {
            public string btcPublic { get; set; }
            public string ethPublic { get; set; }
        }
    }
}
