using Lykke.Service.IcoApi.Core.Services;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;

namespace Lykke.Service.IcoApi.Services
{
    public class EthService : IEthService
    {
        public string GeneratePublicKey()
        {
            return EthECKey.GenerateKey().GetPubKey().ToHex(true);
        }

        public string GetAddressByPublicKey(string key)
        {
            return new EthECKey(key.HexToByteArray(), false).GetPublicAddress();
        }

        public bool ValidateAddress(string address)
        {
            var util = new AddressUtil();

            try
            {
                // force investors to use checksum addresses only
                return
                    (util.IsValidAddressLength(address) || util.IsValidAddressLength(util.ConvertToValid20ByteAddress(address))) &&
                    (util.IsChecksumAddress(address) || util.IsChecksumAddress(util.ConvertToChecksumAddress(address)));
            }
            catch
            {
                return false;
            }
        }
    }
}
