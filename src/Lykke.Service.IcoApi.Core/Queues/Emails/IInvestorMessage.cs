namespace Lykke.Service.IcoApi.Core.Queues.Emails
{
    public interface IInvestorMessage : IMessage
    {
        string EmailTo { get; set; }
    }
}
