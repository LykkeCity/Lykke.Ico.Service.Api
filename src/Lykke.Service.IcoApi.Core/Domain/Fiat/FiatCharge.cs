namespace Lykke.Service.IcoApi.Core.Domain.Fiat
{
    public class FiatCharge
    {
        public FiatChargeStatus Status { get; set; }

        public string FailureCode { get; set; }

        public string FailureMessage { get; set; }
    }
}
