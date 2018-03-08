namespace Lykke.Service.IcoApi.Core.Emails
{
    [EmailData("new-transaction")]
    public class NewTransaction
    {
        public NewTransaction()
        {
            AuthToken = LinkTransactionDetails = KycLink = TransactionAsset = string.Empty;
        }

        public string AuthToken { get; set; }
        public string LinkTransactionDetails { get; set; }
        public bool KycRequired { get; set; }
        public string KycLink { get; set; }
        public bool MoreInvestmentRequired { get; set; }
        public decimal MinAmount { get; set; }

        public decimal InvestorAmountUsd { get; set; }
        public decimal InvestorAmountSmarcToken { get; set; }
        public decimal InvestorAmountLogiToken { get; set; }

        public decimal TransactionAmount { get; set; }
        public decimal TransactionAmountUsd { get; set; }
        public decimal TransactionAmountSmarcToken { get; set; }
        public decimal TransactionAmountLogiToken { get; set; }
        public decimal TransactionFee { get; set; }
        public string TransactionAsset { get; set; }
    }
}
