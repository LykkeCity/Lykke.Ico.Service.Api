using Common;
using Common.Log;
using System;
using System.Threading.Tasks;
using Lykke.Service.IcoApi.Core.Services;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoCommon.Client;
using Lykke.Service.IcoCommon.Client.Models;
using Lykke.Service.IcoApi.Core.Repositories;
using Lykke.Service.IcoApi.Core.Emails;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Domain.Campaign;
using Lykke.Service.IcoApi.Core.Domain.AddressPool;

namespace Lykke.Service.IcoJob.Services
{
    public class InvestorJobService : IInvestorJobService
    {
        private readonly ILog _log;
        private readonly IBtcService _btcService;
        private readonly IEthService _ethService;
        private readonly IInvestorRepository _investorRepository;
        private readonly IAddressPoolRepository _addressPoolRepository;
        private readonly IAddressPoolHistoryRepository _addressPoolHistoryRepository;
        private readonly ICampaignInfoRepository _campaignInfoRepository;
        private readonly IIcoCommonServiceClient _icoCommonServiceClient;

        public InvestorJobService(ILog log,
            IBtcService btcService,
            IEthService ethService,
            IInvestorRepository investorRepository,
            IInvestorAttributeRepository investorAttributeRepository,
            IAddressPoolRepository addressPoolRepository,
            IAddressPoolHistoryRepository addressPoolHistoryRepository,
            ICampaignInfoRepository campaignInfoRepository,
            IIcoCommonServiceClient icoCommonServiceClient)
        {
            _log = log;
            _btcService = btcService;
            _ethService = ethService;
            _investorRepository = investorRepository;
            _addressPoolRepository = addressPoolRepository;
            _addressPoolHistoryRepository = addressPoolHistoryRepository;
            _campaignInfoRepository = campaignInfoRepository;
            _icoCommonServiceClient = icoCommonServiceClient;
        }

        public async Task AssignPayInAddresses(string email)
        {
            email = email.ToLowCase();

            var investor = await _investorRepository.GetAsync(email);
            if (investor.PayInAssigned)
            {
                await _log.WriteInfoAsync(nameof(InvestorJobService), nameof(AssignPayInAddresses),
                    $"email={email}", "Pay-in addresses are already assigned");

                return;
            }

            var poolItemSmarc = await GetNextPoolItem(email);
            var poolItemLogi = await GetNextPoolItem(email);
            var poolItemSmarc90Logi10 = await GetNextPoolItem(email);

            var payInAddresses = new InvestorPayInAddresses
            {
                PayInSmarcEthPublicKey = poolItemSmarc.EthPublicKey,
                PayInSmarcEthAddress = _ethService.GetAddressByPublicKey(poolItemSmarc.EthPublicKey),
                PayInSmarcBtcPublicKey = poolItemSmarc.BtcPublicKey,
                PayInSmarcBtcAddress = _btcService.GetAddressByPublicKey(poolItemSmarc.BtcPublicKey),
                PayInLogiEthPublicKey = poolItemLogi.EthPublicKey,
                PayInLogiEthAddress = _ethService.GetAddressByPublicKey(poolItemLogi.EthPublicKey),
                PayInLogiBtcPublicKey = poolItemLogi.BtcPublicKey,
                PayInLogiBtcAddress = _btcService.GetAddressByPublicKey(poolItemLogi.BtcPublicKey),
                PayInSmarc90Logi10EthPublicKey = poolItemSmarc90Logi10.EthPublicKey,
                PayInSmarc90Logi10EthAddress = _ethService.GetAddressByPublicKey(poolItemSmarc90Logi10.EthPublicKey),
                PayInSmarc90Logi10BtcPublicKey = poolItemSmarc90Logi10.BtcPublicKey,
                PayInSmarc90Logi10BtcAddress = _btcService.GetAddressByPublicKey(poolItemSmarc90Logi10.BtcPublicKey),
            };

            await _log.WriteInfoAsync(nameof(InvestorJobService), nameof(AssignPayInAddresses),
                $"email={email}, payInAddresses={payInAddresses.ToJson()}",
                "Save investor pay-in addresses data");

            await _investorRepository.SavePayInAddressesAsync(email, payInAddresses);

            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInSmarcEthAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Eth,
                Email = email
            });
            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInSmarcBtcAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Btc,
                Email = email
            });

            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInLogiEthAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Eth,
                Email = email
            });
            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInLogiBtcAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Btc,
                Email = email
            });

            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInSmarc90Logi10EthAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Eth,
                Email = email
            });
            await _icoCommonServiceClient.AddPayInAddressAsync(new PayInAddressModel
            {
                Address = payInAddresses.PayInSmarc90Logi10BtcAddress,
                CampaignId = Consts.CAMPAIGN_ID,
                Currency = IcoCommon.Client.Models.CurrencyType.Btc,
                Email = email
            });

            await _campaignInfoRepository.IncrementValue(CampaignInfoType.InvestorsFilledIn, 1);

            await SendSummaryEmail(email);
        }

        private async Task<IAddressPoolItem> GetNextPoolItem(string email)
        {
            var addressPoolNextIdStr = await _campaignInfoRepository.GetValueAsync(CampaignInfoType.AddressPoolNextId);
            if (Int32.TryParse(addressPoolNextIdStr, out var addressPoolNextId))
            {
                addressPoolNextId = 1;
            }

            var poolItem = await _addressPoolRepository.Get(addressPoolNextId);
            if (poolItem == null)
            {
                throw new Exception($"There are no free addresses in address pool. addressPoolNextId={addressPoolNextId}");
            }

            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolNextId, 1);
            await _campaignInfoRepository.IncrementValue(CampaignInfoType.AddressPoolCurrentSize, -1);
            await _addressPoolHistoryRepository.SaveAsync(poolItem, email);

            return poolItem;
        }

        private async Task SendSummaryEmail(string email)
        {
            var investor = await _investorRepository.GetAsync(email);

            var message = new Summary
            {
                AuthToken = investor.ConfirmationToken.Value.ToString(),
                TokenAddress = investor.TokenAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                RefundEthAddress = investor.RefundEthAddress
            };

            await _icoCommonServiceClient.SendEmailAsync(new EmailDataModel
            {
                To = investor.Email,
                TemplateId = "summary",
                CampaignId = Consts.CAMPAIGN_ID,
                Data = message
            });
        }
    }
}
