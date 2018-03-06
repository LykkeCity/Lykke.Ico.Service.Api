namespace Lykke.Service.IcoApi.Core.Emails
{
    public class InvestorSummaryMessage
    {
        public string AuthToken { get; set; }
        public string TokenAddress { get; set; }
        public string RefundBtcAddress { get; set; }
        public string RefundEthAddress { get; set; }
    }
}
