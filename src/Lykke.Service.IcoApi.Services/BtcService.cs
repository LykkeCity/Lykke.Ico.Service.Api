using Lykke.Service.IcoApi.Core.Services;
using NBitcoin;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.IcoApi.Services
{
    public class BtcService : IBtcService
    {
        private readonly Network _btcNetwork;
        private readonly string _testSecretKey;

        public BtcService(string btcNetwork, string testSecretKey)
        {
            _btcNetwork = Network.GetNetwork(btcNetwork);
            _testSecretKey = testSecretKey;
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

        public decimal GetBalance(string address)
        {
            var client = new QBitNinjaClient(_btcNetwork);
            var bitcoinAddress = BitcoinAddress.Create(address);

            var balanceModel = client.GetBalance(bitcoinAddress, true).Result;
            var unspentCoins = new List<Coin>();
            foreach (var operation in balanceModel.Operations)
            {
                unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
            }

            return unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
        }

        public string SendBtcToAddress(string address, decimal amount)
        {
            var destinationAddress = BitcoinAddress.Create(address);
            var testWallet = new BitcoinSecret(_testSecretKey);
            var client = new QBitNinjaClient(_btcNetwork);

            var unspentCoins = new List<Coin>();
            var balanceModel = client.GetBalance(testWallet, true).Result;
            foreach (var operation in balanceModel.Operations)
            {
                unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin as Coin));
            }

            var balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));

            var txBuilder = new TransactionBuilder();
            var tx = txBuilder
                .AddCoins(unspentCoins)
                .AddKeys(testWallet.PrivateKey)
                .Send(destinationAddress, new Money(amount, MoneyUnit.BTC))
                .SendFees("0.001")
                .SetChange(testWallet)
                .BuildTransaction(true);

            var hex = tx.ToHex();

            var broadcastResponse = client.Broadcast(tx).Result;
            if (!broadcastResponse.Success)
            {
                throw new Exception($"Failed to broadcast. ErrorCode : {broadcastResponse.Error.ErrorCode}, Reason: {broadcastResponse.Error.Reason}");
            }

            return tx.ToString();
        }
    }
}
