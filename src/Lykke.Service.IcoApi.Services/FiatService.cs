using Common;
using Common.Log;
using Lykke.Service.IcoApi.Core.Domain;
using Lykke.Service.IcoApi.Core.Domain.Fiat;
using Lykke.Service.IcoApi.Core.Queues;
using Lykke.Service.IcoApi.Core.Queues.Transactions;
using Lykke.Service.IcoApi.Core.Services;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Services
{
    public class FiatService : IFiatService
    {
        private readonly ILog _log;
        private readonly IQueuePublisher<TransactionMessage> _transactionQueuePublisher;

        public FiatService(ILog log,
            IQueuePublisher<TransactionMessage> transactionQueuePublisher)
        {
            _log = log;
            _transactionQueuePublisher = transactionQueuePublisher;
        }

        public async Task<FiatCharge> Charge(string email, string token, int cents)
        {
            try
            {
                var charge = await ChargeToken(email, token, cents);
                await _log.WriteInfoAsync(nameof(FiatService), nameof(Charge),
                    $"charge.StripeResponse={charge.StripeResponse.ResponseJson}",
                    "Charge object recieved");

                if (!charge.Paid)
                {
                    return new FiatCharge
                    {
                        Status = FiatChargeStatus.Failed,
                        FailureCode = charge.FailureCode,
                        FailureMessage = charge.FailureMessage
                    };
                }

                var amount = Decimal.Round(((decimal)cents / 100), 2);
                var fee = Decimal.Round(((decimal)charge.BalanceTransaction.Fee / 100), 2);

                await SendTxMessageAsync(email, charge.Created.ToUniversalTime(), charge.Id, 
                    amount - fee, fee);

                return new FiatCharge
                {
                    Status = FiatChargeStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new FiatCharge
                {
                    Status = FiatChargeStatus.Failed,
                    FailureMessage = ex.Message
                };
            }
        }

        private async Task<StripeCharge> ChargeToken(string email, string token, int cents)
        {
            var charges = new StripeChargeService { ExpandBalanceTransaction = true };
            var options = new StripeChargeCreateOptions
            {

                Amount = cents,
                Currency = "usd",
                SourceTokenOrExistingSourceId = token,
                Metadata = new Dictionary<String, String>() { { "Email", email } }
            };

            try
            {
                return await charges.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                await _log.WriteErrorAsync(nameof(FiatService), nameof(Charge),
                    $"Failed to charge: {options.ToJson()}. StripeError: {ex.StripeError.ToJson()}",
                    ex);

                throw;
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(FiatService), nameof(Charge),
                    $"Failed to charge: {options.ToJson()}",
                    ex);

                throw;
            }
        }

        private async Task SendTxMessageAsync(string email, DateTime createdUtc, string transactionId,
            decimal amount, decimal fee)
        {
            var message = new TransactionMessage
            {
                Email = email,
                Amount = amount,
                CreatedUtc = createdUtc,
                Currency = CurrencyType.Fiat,
                Fee = fee,
                TransactionId = transactionId,
                UniqueId = transactionId
            };

            await _log.WriteInfoAsync(nameof(FiatService), nameof(SendTxMessageAsync),
                $"message={message.ToJson()}", "Transaction message to send");

            try
            {
                await _transactionQueuePublisher.SendAsync(message);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(FiatService), nameof(Charge),
                    $"Failed to send transaction message", ex);

                throw;
            }
        }
    }
}
