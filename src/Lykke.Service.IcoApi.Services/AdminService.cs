using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Common;
using Common.Log;
using CsvHelper;
using Newtonsoft.Json;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Ico.Core;
using Lykke.Ico.Core.Queues;
using Lykke.Ico.Core.Queues.Transactions;
using Lykke.Ico.Core.Queues.Emails;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.AddressPoolHistory;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using Lykke.Ico.Core.Repositories.InvestorEmail;
using Lykke.Ico.Core.Repositories.InvestorTransaction;
using Lykke.Ico.Core.Repositories.CampaignSettings;
using Lykke.Ico.Core.Repositories.InvestorRefund;
using Lykke.Ico.Core.Repositories.PrivateInvestorAttribute;
using Lykke.Ico.Core.Repositories.PrivateInvestor;
using Lykke.Ico.Core.Services;

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
        private readonly IQueuePublisher<InvestorReferralCodeMessage> _investorReferralCodePublisher;
        private readonly IQueuePublisher<Investor20MFixMessage> _investor20MFixPublisher;
        private readonly IPrivateInvestorRepository _privateInvestorRepository;
        private readonly IPrivateInvestorAttributeRepository _privateInvestorAttributeRepository;
        private readonly IReferralCodeService _referralCodeService;
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
            IQueuePublisher<InvestorReferralCodeMessage> investorReferralCodePublisher,
            IQueuePublisher<Investor20MFixMessage> investor20MFixPublisher,
            IPrivateInvestorRepository privateInvestorRepository,
            IPrivateInvestorAttributeRepository privateInvestorAttributeRepository,
            IReferralCodeService referralCodeService,
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
            _investorReferralCodePublisher = investorReferralCodePublisher;
            _investor20MFixPublisher = investor20MFixPublisher;
            _privateInvestorRepository = privateInvestorRepository;
            _privateInvestorAttributeRepository = privateInvestorAttributeRepository;
            _referralCodeService = referralCodeService;
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
                if (!string.IsNullOrEmpty(inverstor.KycRequestId))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.KycId, inverstor.KycRequestId);
                }
                if (!string.IsNullOrEmpty(inverstor.ReferralCode))
                {
                    await _investorAttributeRepository.RemoveAsync(InvestorAttributeType.ReferralCode, inverstor.ReferralCode);
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

        public async Task<string> Recalculate20MTxs(bool saveChanges)
        {
            var report = new StringBuilder();
            var amountTokens = 20_000_000M;
            var tokenPrice = 0.065M * 0.75M;
            var amountUsd = amountTokens * tokenPrice;
            var settings = await _campaignSettingsRepository.GetAsync();

            report.AppendLine("Start");
            report.AppendLine($"- When (UTC): {DateTime.UtcNow}");
            report.AppendLine($"- Tokens Amount: {amountTokens}");
            report.AppendLine($"- Token Price: {tokenPrice}");
            report.AppendLine($"- Usd Amount: {amountUsd}");

            report.AppendLine("");
            report.AppendLine("Transactions");

            var txsAll = await _investorTransactionRepository.GetAllAsync();
            var txsAllOld = await _investorTransactionRepository.GetAllAsync();
            var investors = await _investorRepository.GetAllAsync();

            report.AppendLine($"- Total txs: {txsAll.Count()}");

            var txsCrowdSale = txsAll
                .Where(f => f.CreatedUtc > DateTime.Parse("2018-02-24T00:00:00.000Z"))
                .OrderBy(f => f.CreatedUtc)
                .ThenBy(f => f.ProcessedUtc);
            report.AppendLine($"- Crowdsale txs: {txsCrowdSale.Count()}");

            var txs = new List<IInvestorTransaction>();
            foreach (var txCrowdSale in txsCrowdSale)
            {
                if (txs.Sum(f => f.AmountUsd) < amountUsd)
                {
                    txs.Add(txCrowdSale);
                }
            }
            report.AppendLine($"- 20M txs: {txs.Count()}");

            var totalUsd20M = 0M;
            var totalTokens20M = 0M;
            var totalTokens20MOld = 0M;
            foreach (var tx in txs)
            {
                totalUsd20M += tx.AmountUsd;
                totalTokens20MOld += tx.AmountToken;

                var tokenPriceList = TokenPrice.GetPriceList(settings, tx.CreatedUtc, tx.AmountUsd, totalTokens20M);
                var txTokenAmountNew = tokenPriceList.Sum(p => p.Count);
                var tokenPriceNew = tokenPriceList.Count == 1 ? tokenPriceList[0].Price : tx.AmountUsd / txTokenAmountNew;

                totalTokens20M += txTokenAmountNew;

                report.AppendLine($"  - tx: createdUtc={tx.CreatedUtc.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK")}, email={tx.Email}");
                report.AppendLine($"    AmountToken: old={tx.AmountToken}, new={txTokenAmountNew}, diff={txTokenAmountNew - tx.AmountToken}");
                report.AppendLine($"    TokenPrice: old={tx.TokenPrice}, new={tokenPriceNew}");
                report.AppendLine($"    TokenPriceContext: old={tx.TokenPriceContext}, new={tokenPriceList.ToJson()}");
                report.AppendLine($"    Total USD 20M: {totalUsd20M}");

                tx.AmountToken = txTokenAmountNew;
                tx.TokenPrice = tokenPriceNew;
                tx.TokenPriceContext = tokenPriceList.ToJson();

                if (saveChanges)
                {
                    await _investorTransactionRepository.SaveAsync(tx);
                }
            }

            report.AppendLine($"- Total tokens 20M (old): {totalTokens20MOld}");
            report.AppendLine($"- Total tokens 20M (new): {totalTokens20M}");

            var investor20MFixMessages = new List<Investor20MFixMessage>();
            foreach (var tx in txs)
            {
                var investor20MFixMessage = investor20MFixMessages.FirstOrDefault(f => f.EmailTo == tx.Email);
                if (investor20MFixMessage == null)
                {
                    var investor = investors.First(f => f.Email == tx.Email);

                    investor20MFixMessages.Add(new Investor20MFixMessage
                    {
                        EmailTo = tx.Email,
                        LinkToSummaryPage = _icoApiSettings.SiteSummaryPageUrl.Replace("{token}", investor.ConfirmationToken.Value.ToString()),
                        OldTokens = investor.AmountToken
                    });
                }
            }

            report.AppendLine("");
            report.AppendLine("Investors");

            foreach (var tx in txs)
            {
                var txOld = txsAllOld.First(f => f.UniqueId == tx.UniqueId);
                var investor = investors.First(f => f.Email == tx.Email);
                var diffTokens = tx.AmountToken - txOld.AmountToken;

                report.AppendLine($"- {investor.Email}: " +
                    $"old={investor.AmountToken}, " +
                    $"new={investor.AmountToken + diffTokens}, " +
                    $"diff={diffTokens}");

                if (saveChanges)
                {
                    await _investorRepository.IncrementTokens(investor.Email, diffTokens);
                }
            }

            report.AppendLine("");
            report.AppendLine("Emails");

            investors = await _investorRepository.GetAllAsync();
            foreach (var tx in txs)
            {
                var investor20MFixMessage = investor20MFixMessages.FirstOrDefault(f => f.EmailTo == tx.Email);
                if (investor20MFixMessage != null)
                {
                    var investor = investors.First(f => f.Email == tx.Email);

                    investor20MFixMessage.NewTokens = investor.AmountToken;
                }
            }

            foreach (var investor20MFixMessage in investor20MFixMessages)
            {
                if (saveChanges)
                {
                    await _investor20MFixPublisher.SendAsync(investor20MFixMessage);

                    report.AppendLine($"- sent: {investor20MFixMessage.ToJson()}");
                }
                else
                {
                    report.AppendLine($"- {investor20MFixMessage.ToJson()}");
                }
            }

            report.AppendLine("");
            report.AppendLine("CampaignInfo");

            var amountInvestedToken = Decimal.Parse(await _campaignInfoRepository.GetValueAsync(CampaignInfoType.AmountInvestedToken));
            var amountInvestedTokenDiff = totalTokens20M - totalTokens20MOld;
            var amountInvestedTokenNew = amountInvestedToken + amountInvestedTokenDiff;

            report.AppendLine($"- AmountInvestedToken (old): {amountInvestedToken}");
            report.AppendLine($"- AmountInvestedToken (new): {amountInvestedTokenNew}");
            report.AppendLine($"- AmountInvestedToken (diff): {amountInvestedTokenDiff}");

            if (saveChanges)
            {
                await _campaignInfoRepository.IncrementValue(CampaignInfoType.AmountInvestedToken, amountInvestedTokenDiff);
            }

            return report.ToString();
        }

        public async Task Send20MFixEmail(string email, decimal oldToken, decimal newTokens)
        {
            await _investor20MFixPublisher.SendAsync(new Investor20MFixMessage
            {
                EmailTo = email,
                LinkToSummaryPage = _icoApiSettings.SiteSummaryPageUrl.Replace("{token}", Guid.NewGuid().ToString()),
                OldTokens = oldToken,
                NewTokens = newTokens,
            });
        }

        public class TokenPrice
        {
            public TokenPrice(decimal count, decimal price, string phase)
            {
                Count = count;
                Price = price;
                Phase = phase;
            }

            public decimal Count { get; }
            public decimal Price { get; }
            public string Phase { get; }

            public static IList<TokenPrice> GetPriceList(ICampaignSettings campaignSettings, DateTime txDateTimeUtc,
                decimal amountUsd, decimal currentTotal)
            {
                var tokenInfo = campaignSettings.GetTokenInfo(currentTotal, txDateTimeUtc);
                if (tokenInfo == null)
                {
                    return null;
                }

                var priceList = new List<TokenPrice>();
                var tokenPhase = Enum.GetName(typeof(TokenPricePhase), tokenInfo.Phase);
                var tokens = RoundDown((amountUsd / tokenInfo.Price), campaignSettings.TokenDecimals);

                if (tokenInfo.Phase == TokenPricePhase.CrowdSaleInitial)
                {
                    var tokensBelow = Consts.CrowdSale.InitialAmount - currentTotal;
                    if (tokensBelow > 0M)
                    {
                        if (tokens > tokensBelow)
                        {
                            // tokens below threshold
                            priceList.Add(new TokenPrice(tokensBelow, tokenInfo.Price, tokenPhase));

                            // tokens above threshold
                            var amountUsdAbove = amountUsd - (tokensBelow * tokenInfo.Price);
                            var priceAbove = campaignSettings.GetTokenPrice(TokenPricePhase.CrowdSaleFirstDay);
                            var tokensAbove = RoundDown((amountUsdAbove / priceAbove), campaignSettings.TokenDecimals);

                            priceList.Add(new TokenPrice(tokensAbove, priceAbove, nameof(TokenPricePhase.CrowdSaleFirstDay)));

                            return priceList;
                        }
                    }
                }

                priceList.Add(new TokenPrice(tokens, tokenInfo.Price, tokenPhase));

                return priceList;
            }

            public static decimal RoundDown(decimal self, double decimalPlaces)
            {
                var power = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
                var value = Math.Floor(self * power) / power;

                return value;
            }
        }

        public async Task<IEnumerable<(string Email, string Code)>> GenerateReferralCodes()
        {
            var result = new List<(string Email, string Code)>();
            var settings = await _campaignSettingsRepository.GetAsync();

            if (!settings.ReferralCodeLength.HasValue)
            {
                throw new Exception("settings.ReferralCodeLength does not have value");
            }

            var investors = await _investorRepository.GetAllAsync();
            foreach (var investor in investors)
            {
                if (string.IsNullOrEmpty(investor.ReferralCode) && 
                    investor.AmountUsd >= settings.MinInvestAmountUsd)
                {
                    var code = await _referralCodeService.GetReferralCode(settings.ReferralCodeLength.Value);
                    await _investorRepository.SaveReferralCode(investor.Email, code);
                    await _investorAttributeRepository.SaveAsync(InvestorAttributeType.ReferralCode,
                        investor.Email, code);

                    result.Add((investor.Email, code));
                }
            }

            var privateInvestors = await _privateInvestorRepository.GetAllAsync();
            foreach (var privateInvestor in privateInvestors)
            {
                if (string.IsNullOrEmpty(privateInvestor.ReferralCode))
                {
                    var code = await _referralCodeService.GetReferralCode(settings.ReferralCodeLength.Value);
                    await _privateInvestorRepository.SaveReferralCode(privateInvestor.Email, code);
                    await _privateInvestorAttributeRepository.SaveAsync(PrivateInvestorAttributeType.ReferralCode,
                        privateInvestor.Email, code);

                    result.Add((privateInvestor.Email, code));
                }
            }

            return result;
        }

        public async Task SendEmailWithReferralCode(IInvestor investor)
        {
            await _investorReferralCodePublisher.SendAsync(new InvestorReferralCodeMessage
            {
                EmailTo = investor.Email,
                ReferralCode = investor.ReferralCode,
                LinkToSummaryPage = _icoApiSettings.SiteSummaryPageUrl.Replace("{token}", investor.ConfirmationToken.Value.ToString())
            });
        }
    }
}
