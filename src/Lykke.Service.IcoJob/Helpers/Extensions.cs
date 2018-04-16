using Common;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Investor;
using Lykke.Service.IcoApi.Core.Queues.Messages;
using System;

namespace Lykke.Service.IcoJob.Helpers
{
    public static class Extensions
    {
        public static TxType GetTxType(this TransactionMessage msg, IInvestor investor)
        {
            if (msg.Currency == CurrencyType.Bitcoin || msg.Currency == CurrencyType.Ether)
            {
                if (investor.PayInSmarcEthAddress == msg.PayInAddress ||
                    investor.PayInSmarcBtcAddress == msg.PayInAddress)
                {
                    return TxType.Smarc;
                }

                if (investor.PayInLogiEthAddress == msg.PayInAddress ||
                    investor.PayInLogiBtcAddress == msg.PayInAddress)
                {
                    return TxType.Logi;
                }

                if (investor.PayInSmarc90Logi10EthAddress == msg.PayInAddress ||
                    investor.PayInSmarc90Logi10BtcAddress == msg.PayInAddress)
                {
                    return TxType.Smarc90Logi10;
                }

                throw new Exception($"Pay-in address={msg.PayInAddress} not found in investor={investor.ToJson()}");
            }

            if (msg.Currency == CurrencyType.Fiat)
            {
                if (!msg.Type.HasValue)
                {
                    throw new Exception($"msg.Type can not be null for the Fiat currency type. msg={msg.ToJson()}");
                }

                return msg.Type.Value;
            }

            throw new Exception($"Unsupported currency for msg={msg.ToJson()}");
        }
    }    
}
