namespace Lykke.Service.IcoApi.Core.Queues.Emails
{
    public class InvestorSummaryMessage
    {
        public string LinkToSummaryPage { get; set; }
        public string TokenAddress { get; set; }
        public string RefundBtcAddress { get; set; }
        public string RefundEthAddress { get; set; }
        public string LinkEthAddress { get; set; }
        public string LinkBtcAddress { get; set; }
    }
}   
