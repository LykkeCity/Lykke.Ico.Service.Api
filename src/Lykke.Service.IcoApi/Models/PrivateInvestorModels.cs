using Lykke.Ico.Core.Repositories.PrivateInvestor;
using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class PrivateInvestorResponse
    {
        public string Email { get; set; }

        public string KycId { get; set; }

        public KycStatus KycStatus { get; set; }

        public string KycLink { get; set; }

        DateTime UpdatedUtc { get; set; }

        public DateTime? KycPassedUtc { get; set; }

        public DateTime? KycManuallyUpdatedUtc { get; set; }

        public string ReferralCode { get; set; }

        public DateTime? ReferralCodeUtc { get; set; }

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
                KycLink = kycLink,
                UpdatedUtc = investor.UpdatedUtc,
                KycPassedUtc = investor.KycPassedUtc,
                KycManuallyUpdatedUtc = investor.KycManuallyUpdatedUtc,
                ReferralCode = investor.ReferralCode,
                ReferralCodeUtc = investor.ReferralCodeUtc
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
