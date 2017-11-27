using Common.Log;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Lykke.Ico.Core.Repositories.AddressPool;
using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Lykke.Ico.Core.Repositories.AddressPoolHistory;
using Lykke.Ico.Core.Repositories.CampaignInfo;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Text;

namespace Lykke.Service.IcoApi.Services
{
    public class AdminService : IAdminService
    {
        private readonly string _btcNetwork;
        private readonly string _ethNetwork;
        private readonly ILog _log;
        private readonly IInvestorRepository _investorRepository;
        private readonly IInvestorAttributeRepository _investorAttributeRepository;
        private readonly IEmailHistoryRepository _emailHistoryRepository;
        private readonly IInvestorHistoryRepository _investorHistoryRepository;
        private readonly IAddressPoolHistoryRepository _addressPoolHistoryRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;

        public AdminService(string btcNetwork,
            string ethNetwork,
            ILog log,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IEmailHistoryRepository emailHistoryRepository,
            IInvestorHistoryRepository investorHistoryRepository,
            IAddressPoolHistoryRepository addressPoolHistoryRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IAddressPoolRepository addressPoolRepository)
        {
            _btcNetwork = btcNetwork;
            _ethNetwork = ethNetwork;
            _log = log;
            _investorRepository = investorRepository;
            _investorAttributeRepository = investorAttributeRepository;
            _emailHistoryRepository = emailHistoryRepository;
            _investorHistoryRepository = investorHistoryRepository;
            _addressPoolHistoryRepository = addressPoolHistoryRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _addressPoolRepository = addressPoolRepository;
        }

        public async Task<Dictionary<string, string>> GetCampainInfo()
        {
            var dictionary = await _campaignInfoRepository.GetAllAsync();

            dictionary.Add("BctNetwork", _btcNetwork);
            dictionary.Add("EthNetwork", _ethNetwork);

            return dictionary;
        }

        public async Task DeleteInvestorAsync(string email)
        {
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

        public async Task<IEnumerable<IInvestorHistoryItem>> GetInvestorHistory(string email)
        {
            return await _investorHistoryRepository.GetAsync(email);
        }

        public async Task DeleteInvestorHistoryAsync(string email)
        {
            await _emailHistoryRepository.RemoveAsync(email);
            await _investorHistoryRepository.RemoveAsync(email);
            await _addressPoolHistoryRepository.RemoveAsync(email);
        }

        public async Task<IEnumerable<IEmailHistoryItem>> GetInvestorEmails(string email)
        {
            return await _emailHistoryRepository.GetAsync(email);
        }

        public async Task<int> ImportPublicKeys(StreamReader reader)
        {
            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), $"Start of public keys import");

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
                    await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), $"{counter} imported keys");
                }
            }

            if (list.Count > 0)
            {
                await _addressPoolRepository.AddBatchAsync(list);
            }

            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), $"{counter} imported keys");
            await _log.WriteInfoAsync(nameof(AdminService), nameof(ImportPublicKeys), $"End of public keys import");

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
