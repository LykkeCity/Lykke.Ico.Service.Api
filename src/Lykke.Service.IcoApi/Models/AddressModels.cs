using Newtonsoft.Json;

namespace Lykke.Service.IcoApi.Models
{
    public class AddressResponse
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
