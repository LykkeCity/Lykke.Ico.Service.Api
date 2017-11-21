using Lykke.Service.IcoApi.Core.Services;
using NBitcoin;

namespace Lykke.Service.IcoApi.Services
{
    public class BtcService : IBtcService
    {
        private readonly Network _btcNetwork;

        public BtcService(string btcNetwork)
        {
            _btcNetwork = Network.GetNetwork(btcNetwork);
        }

        public string GetAddressByPublicKey(string key)
        {
            return new PubKey(key).GetAddress(_btcNetwork).ToString();
        }

        public bool ValidateAddress(string address)
        {
            try
            {
                return BitcoinAddress.Create(address).Network == _btcNetwork;
            }
            catch
            {
                return false;
            }
        }

        public string GeneratePublicKey()
        {
            return new Key().PubKey.ToHex();
        }
    }
}
