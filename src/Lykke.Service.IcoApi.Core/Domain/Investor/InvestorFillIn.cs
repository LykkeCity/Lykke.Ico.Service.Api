namespace Lykke.Service.IcoApi.Core.Domain.Investor
{
    public class InvestorFillIn
    {
        public string TokenAddress { get; set; }
        public string RefundEthAddress { get; set; }
        public string RefundBtcAddress { get; set; }

        public string PayInEthPublicKey { get; set; }
        public string PayInEthAddress { get; set; }
        public string PayInBtcPublicKey { get; set; }
        public string PayInBtcAddress { get; set; }
    }
}
