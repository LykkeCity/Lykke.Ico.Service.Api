namespace Lykke.Service.IcoApi.Core.Domain
{
    public class FiatCharge
    {
        public FiatChargeStatus Status { get; set; }

        public string FailureCode { get; set; }

        public string FailureMessage { get; set; }
    }
}
