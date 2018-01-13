using System;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.IcoApi.Models
{
    public class KycRequest
    {
        [Required]
        public string Message { get; set; }
    }

    public class KycMessage
    {
        public Guid KycId { get; set; }
        public string KycStatus { get; set; }
    }
}
