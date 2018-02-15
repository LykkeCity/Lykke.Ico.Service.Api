using Lykke.Service.IcoApi.Core.Domain;

namespace Lykke.Service.IcoApi.Core.Queues.Emails
{
    [QueueMessage(QueueName = Consts.Emails.Queues.InvestorConfirmation)]
    public class InvestorConfirmationMessage : IInvestorMessage
    {
        public string EmailTo { get; set; }
        public string ConfirmationLink { get; set; }
    }
}
