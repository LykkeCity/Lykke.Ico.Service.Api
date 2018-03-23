using Lykke.Service.IcoApi.Core.Services;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class EthService : IEthService
    {
        private readonly string _ethNetworkUrl;
        private readonly string _testSecretKey;

        public EthService()
        {

        }

        public EthService(string ethNetworkUrl, string testSecretKey)
        {
            _testSecretKey = testSecretKey;
            _ethNetworkUrl = ethNetworkUrl;
        }

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

        public async Task<decimal> GetBalance(string address)
        {
            var web3 = new Web3(_ethNetworkUrl);

            var wei = await web3.Eth.GetBalance.SendRequestAsync(address);
            var ethers = Web3.Convert.FromWei(wei);

            return ethers;
        }

        public async Task<string> SendToAddress(string address, decimal amount)
        {
            var web3 = new Web3(_ethNetworkUrl);
            var senderAddress = Web3.GetAddressFromPrivateKey(_testSecretKey);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderAddress);
            var wei = Web3.Convert.ToWei(amount);
            var encoded = Web3.OfflineTransactionSigner.SignTransaction(_testSecretKey, address, wei, txCount.Value);
            var txId = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);

            return txId;
        }
    }
}
