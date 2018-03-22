using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData(Consts.Emails.Summary)]
    public class Summary
    {
        public Summary()
        {
            TokenName = AuthToken = TokenAddress = RefundBtcAddress = RefundEthAddress = string.Empty;
        }

        public string TokenName { get; set; }
        public string AuthToken { get; set; }
        public string TokenAddress { get; set; }
        public string RefundBtcAddress { get; set; }
        public string RefundEthAddress { get; set; }
    }
}
