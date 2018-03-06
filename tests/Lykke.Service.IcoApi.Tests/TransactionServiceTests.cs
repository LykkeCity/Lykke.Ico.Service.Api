//using Moq;
//using Xunit;
//using System;
//using System.Threading.Tasks;
//using Common.Log;
//using Lykke.Service.IcoExRate.Client;
//using Lykke.Service.IcoApi.Core.Repositories;
//using Lykke.Service.IcoApi.Core.Domain.Campaign;
//using Lykke.Service.IcoApi.Core.Domain.Investor;
//using Lykke.Service.IcoApi.Core.Services;
//using Lykke.Service.IcoExRate.Client.AutorestClient.Models;
//using Lykke.Service.IcoApi.Services;
//using Lykke.Service.IcoApi.Core.Queues.Messages;
//using Common;
//using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
//using Lykke.Service.IcoCommon.Client;
//using Lykke.Service.IcoCommon.Client.Models;

//namespace Lykke.Job.IcoInvestment.Tests
//{
//    public class TransactionServiceTests
//    {
//        private ILog _log;
//        private Mock<IIcoExRateClient> _exRateClient;
//        private Mock<IInvestorAttributeRepository> _investorAttributeRepository;
//        private Mock<ICampaignInfoRepository> _campaignInfoRepository;
//        private Mock<ICampaignSettingsRepository> _campaignSettingsRepository;
//        private Mock<IInvestorTransactionRepository> _investorTransactionRepository;
//        private Mock<IInvestorRefundRepository> _investorRefundRepository;
//        private Mock<IInvestorRepository> _investorRepository;
//        private Mock<IIcoCommonServiceClient> _icoCommonServiceClient;
//        private ICampaignSettings _campaignSettings;
//        private IInvestor _investor;
//        private IInvestorTransaction _investorTransaction;
//        private IUrlEncryptionService _urlEncryptionService;
//        private IKycService _kycService;
//        private decimal _usdAmount = decimal.Zero;

//        private TransactionService Init(string investorEmail = "test@test.test", double exchangeRate = 1.0)
//        {
//            _log = new LogToMemory();

//            _campaignInfoRepository = new Mock<ICampaignInfoRepository>();

//            _campaignInfoRepository
//                .Setup(m => m.IncrementValue(It.Is<CampaignInfoType>(t => t == CampaignInfoType.AmountInvestedUsd), It.IsAny<decimal>()))
//                .Callback((CampaignInfoType t, decimal v) => _usdAmount += v)
//                .Returns(() => Task.CompletedTask);

//            _campaignInfoRepository
//                .Setup(m => m.SaveLatestTransactionsAsync(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(() => Task.CompletedTask);

//            _campaignSettings = new CampaignSettings
//            {
//                PreSaleStartDateTimeUtc = DateTime.UtcNow.AddDays(-15),
//                PreSaleEndDateTimeUtc = DateTime.UtcNow,
//                PreSaleTotalTokensAmount = 100000000,
//                CrowdSaleStartDateTimeUtc = DateTime.UtcNow,
//                CrowdSaleEndDateTimeUtc = DateTime.UtcNow.AddDays(21),
//                CrowdSaleTotalTokensAmount = 360000000,
//                TokenDecimals = 4,
//                MinInvestAmountUsd = 1000,
//                TokenBasePriceUsd = 0.064M
//            };

//            _campaignSettingsRepository = new Mock<ICampaignSettingsRepository>();

//            _campaignSettingsRepository
//                .Setup(m => m.GetAsync())
//                .Returns(() => Task.FromResult(_campaignSettings));

//            _exRateClient = new Mock<IIcoExRateClient>();

//            _exRateClient
//                .Setup(m => m.GetAverageRate(It.IsAny<Pair>(), It.IsAny<DateTime>()))
//                .Returns(() => Task.FromResult(new AverageRateResponse { AverageRate = exchangeRate }));

//            _investor = new Investor() { Email = investorEmail, ConfirmationToken = Guid.NewGuid() };         

//            _investorRepository = new Mock<IInvestorRepository>();

//            _investorRepository
//                .Setup(m => m.GetAsync(It.Is<string>(v => !string.IsNullOrWhiteSpace(v) && v == investorEmail)))
//                .Returns(() => Task.FromResult(_investor));

//            _investorAttributeRepository = new Mock<IInvestorAttributeRepository>();

//            _investorAttributeRepository
//                .Setup(m => m.GetInvestorEmailAsync(
//                    It.IsIn(new InvestorAttributeType[] { InvestorAttributeType.PayInBtcAddress, InvestorAttributeType.PayInEthAddress }), 
//                    It.IsAny<string>()))
//                .Returns(() => Task.FromResult(investorEmail));

//            _investorTransaction = new InvestorTransaction { };

//            _investorTransactionRepository = new Mock<IInvestorTransactionRepository>();

//            _investorTransactionRepository
//                .Setup(m => m.GetAsync(
//                    It.Is<string>(v => !string.IsNullOrWhiteSpace(v) && v == "test-1@test.test"),
//                    It.IsAny<string>()))
//                .Returns(() => Task.FromResult(_investorTransaction));

//            _investorTransactionRepository
//                .Setup(m => m.SaveAsync(It.IsAny<IInvestorTransaction>()))
//                .Returns(() => Task.CompletedTask);

//            _investorRefundRepository = new Mock<IInvestorRefundRepository>();

//            _investorRefundRepository
//                .Setup(m => m.SaveAsync(It.IsAny<string>(), It.IsAny<InvestorRefundReason>(), It.IsAny<string>()))
//                .Returns(() => Task.CompletedTask);

//            _icoCommonServiceClient = new Mock<IIcoCommonServiceClient>();

//            _icoCommonServiceClient
//                .Setup(m => m.SendEmailAsync(It.IsAny<EmailDataModel>()))
//                .Returns(() => Task.CompletedTask);

//            _urlEncryptionService = new UrlEncryptionService("E546C8DF278CD5931069B522E695D4F2", "1234567890123456");
//            _kycService = new KycService(_campaignSettingsRepository.Object, _urlEncryptionService);

//            return new TransactionService(
//                _log,
//                _exRateClient.Object,
//                _investorAttributeRepository.Object,
//                _campaignInfoRepository.Object,
//                _campaignSettingsRepository.Object,
//                _investorTransactionRepository.Object,
//                _investorRefundRepository.Object,
//                _investorRepository.Object,
//                _kycService,
//                _icoCommonServiceClient.Object,
//                new IcoApiSettings());
//        }

//        [Fact]
//        public async void ShouldProcessMessage()
//        {
//            // Arrange
//            var testExchangeRate = 2M;
//            var testAmount = 1M;
//            var testAmountUsd = testAmount * testExchangeRate;
//            var testBlockId = "testBlock";
//            var testBlockTimestamp = DateTimeOffset.Now;
//            var testAddress = "testAddress";
//            var testTransactionId = "testTransaction-1";
//            var uniqueId = "testTransaction";
//            var testEmail = "test@test.test";
//            var testLink = "testLink";
//            var testCurrency = Service.IcoApi.Core.Domain.CurrencyType.Bitcoin;
//            var svc = Init(testEmail, Decimal.ToDouble(testExchangeRate));

//            // Act
//            await svc.Process(new TransactionMessage
//            {
//                Email = testEmail,
//                Amount = testAmount,
//                BlockId = testBlockId,
//                CreatedUtc = testBlockTimestamp.UtcDateTime,
//                Currency = testCurrency,
//                PayInAddress = testAddress,
//                Link = testLink,
//                TransactionId = testTransactionId,
//                UniqueId = uniqueId
//            });

//            // Assert

//            // History saved
//            _investorTransactionRepository.Verify(m => m.SaveAsync(It.IsAny<IInvestorTransaction>()));

//            // Mail sent
//            _icoCommonServiceClient.Verify(m => m.SendEmailAsync(
//                It.Is<Service.IcoCommon.Client.Models.EmailDataModel>(msg => msg.To == testEmail)));

//            // Total amount incremented
//            _campaignInfoRepository.Verify(m => m.IncrementValue(
//                It.Is<CampaignInfoType>(v => v == CampaignInfoType.AmountInvestedUsd),
//                It.Is<decimal>(v => v == testAmountUsd)));

//            Assert.Equal(testAmountUsd, _usdAmount);
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenCampainSettingsNull()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _campaignSettingsRepository
//                .Setup(m => m.GetAsync())
//                .Returns(() => Task.FromResult<ICampaignSettings>(null));

//            // Act
//            var ex = await Assert.ThrowsAsync<AggregateException>(() => svc.Process(message).Wait());

//            Assert.Contains("Campaign settings", ex.Message);
//        }

//        [Fact]
//        public async void ShouldDoIgnoreAlreadySavedTranscation()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _investorTransactionRepository
//                .Setup(m => m.GetAsync(
//                    It.Is<string>(v => v == message.Email),
//                    It.Is<string>(v => v == message.UniqueId)))
//                .Returns(() => Task.FromResult(_investorTransaction));            

//            // Act
//            await svc.Process(message);

//            // Assert
//            _investorTransactionRepository.Verify(m => m.GetAsync(
//                It.IsAny<string>(),
//                It.IsAny<string>()));
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenInvestorNotFound()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _investorRepository
//                .Setup(m => m.GetAsync(It.Is<string>(v => v == message.Email)))
//                .Returns(() => Task.FromResult<IInvestor>(null));

//            // Act
//            var ex = await Assert.ThrowsAsync<AggregateException>(() => svc.Process(message).Wait());

//            Assert.Contains("Investor with email", ex.Message);
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenUniqueIdIsEmpty()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.ToUniversalTime(),
//                UniqueId = ""
//            };

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _investorRepository
//                .Setup(m => m.GetAsync(It.Is<string>(v => v == message.Email)))
//                .Returns(() => Task.FromResult<IInvestor>(null));

//            // Act
//            var ex = await Assert.ThrowsAsync<AggregateException>(() => svc.Process(message).Wait());

//            Assert.Contains("UniqueId can not be empty", ex.Message);
//        }

//        [Fact]
//        public void ShouldThrowExceptionWhenEmailIsEmpty()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "",
//                CreatedUtc = DateTime.Now.ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _investorRepository
//                .Setup(m => m.GetAsync(It.Is<string>(v => v == message.Email)))
//                .Returns(() => Task.FromResult<IInvestor>(null));

//            // Act
//            var ex = await Assert.ThrowsAsync<AggregateException>(() => svc.Process(message).Wait());

//            Assert.Contains("Email can not be empty", ex.Message);
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenTxOutOfDates()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.AddDays(-20).ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var messageJson = message.ToJson();

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            // Act
//            await svc.Process(message);

//            // Assert
//            _investorRefundRepository.Verify(m => m.SaveAsync(
//                It.Is<string>(v => v == message.Email),
//                It.Is<InvestorRefundReason>(v => v == InvestorRefundReason.OutOfDates),
//                It.Is<string>(v => v == messageJson)));
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenWhenAllPresalesTokensSoldOut()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.AddDays(-10).ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var messageJson = message.ToJson();

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _campaignInfoRepository
//                .Setup(m => m.GetValueAsync(It.Is<CampaignInfoType>(t => t == CampaignInfoType.AmountInvestedToken)))
//                .Returns(() => Task.FromResult("200000000"));

//            // Act
//            await svc.Process(message);

//            // Assert
//            _investorRefundRepository.Verify(m => m.SaveAsync(
//                It.Is<string>(v => v == message.Email),
//                It.Is<InvestorRefundReason>(v => v == InvestorRefundReason.PreSaleTokensSoldOut),
//                It.Is<string>(v => v == messageJson)));
//        }

//        [Fact]
//        public async void ShouldThrowExceptionWhenWhenAllTokensSoldOut()
//        {
//            // Arrange
//            var message = new TransactionMessage
//            {
//                Email = "test@test.test",
//                CreatedUtc = DateTime.Now.AddDays(10).ToUniversalTime(),
//                UniqueId = "111"
//            };

//            var messageJson = message.ToJson();

//            var svc = Init(message.Email, Decimal.ToDouble(1M));

//            _campaignInfoRepository
//                .Setup(m => m.GetValueAsync(It.Is<CampaignInfoType>(t => t == CampaignInfoType.AmountInvestedToken)))
//                .Returns(() => Task.FromResult("600000000"));

//            await svc.Process(message);

//            // Assert
//            _investorRefundRepository.Verify(m => m.SaveAsync(
//                It.Is<string>(v => v == message.Email),
//                It.Is<InvestorRefundReason>(v => v == InvestorRefundReason.TokensSoldOut),
//                It.Is<string>(v => v == messageJson)));
//        }
//    }
//}
