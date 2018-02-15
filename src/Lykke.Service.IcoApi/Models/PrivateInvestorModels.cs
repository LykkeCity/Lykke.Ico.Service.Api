using Lykke.Service.IcoApi.Core.Domain.PrivateInvestor;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class PrivateInvestorResponse
    {
        public string Email { get; set; }

        public string KycId { get; set; }

        public KycStatus KycStatus { get; set; }

        public string KycLink { get; set; }

        public static PrivateInvestorResponse Create(IPrivateInvestor investor, string kycLink)
        {
            var kycStatus = KycStatus.None;
            if (investor.KycRequestedUtc.HasValue)
            {
                kycStatus = KycStatus.Requested;
            }
            if (investor.KycPassed.HasValue)
            {
                kycStatus = investor.KycPassed.Value ? KycStatus.Success : KycStatus.Failed;
            }

            return new PrivateInvestorResponse
            {
                Email = investor.Email,
                KycId = investor.KycRequestId,
                KycStatus = kycStatus,
                KycLink = kycLink
            };
        }
    }

    public class CreatePrivateInvestorRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
