﻿namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData("summary")]
    public class Summary
    {
        public Summary()
        {
            AuthToken = TokenAddress = RefundBtcAddress = RefundEthAddress = string.Empty;
        }

        public string AuthToken { get; set; }
        public string TokenAddress { get; set; }
        public string RefundBtcAddress { get; set; }
        public string RefundEthAddress { get; set; }
    }
}