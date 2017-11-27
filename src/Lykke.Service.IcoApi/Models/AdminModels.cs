using Lykke.Ico.Core.Repositories.EmailHistory;
using Lykke.Ico.Core.Repositories.Investor;
using Lykke.Ico.Core.Repositories.InvestorHistory;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.IcoApi.Models
{
    public class FullInvestorResponse
    {
        public string Email { get; set; }

        public string TokenAddress { get; set; }

        public string RefundEthAddress { get; set; }

        public string RefundBtcAddress { get; set; }

        public string PayInEthAddress { get; set; }

        public string PayInBtcAddress { get; set; }

        public Guid? ConfirmationToken { get; set; }

        public DateTime? ConfirmationDateTimeUtc { get; set; }

        public DateTime UpdatedUtc { get; set; }

        public static FullInvestorResponse Create(IInvestor investor)
        {
            return new FullInvestorResponse
            {
                Email = investor.Email,
                TokenAddress = investor.TokenAddress,
                RefundEthAddress = investor.RefundEthAddress,
                RefundBtcAddress = investor.RefundBtcAddress,
                PayInEthAddress = investor.PayInEthAddress,
                PayInBtcAddress = investor.PayInBtcAddress,
                ConfirmationToken = investor.ConfirmationToken,
                ConfirmationDateTimeUtc = investor.ConfirmationDateTimeUtc,
                UpdatedUtc = investor.UpdatedUtc
            };
        }
    }

    public class InvestorHistoryResponse
    {
        public InvestorHistoryModel[] Items { get; set; }

        public static InvestorHistoryResponse Create(IEnumerable<IInvestorHistoryItem> items)
        {
            return new InvestorHistoryResponse { Items = items.Select(f => InvestorHistoryModel.Create(f)).ToArray() };
        }
    }

    public class InvestorHistoryModel : IInvestorHistoryItem
    {
        public string Email { get; set; }

        public DateTime WhenUtc { get; set; }

        public InvestorHistoryAction Action { get; set; }

        public string Json { get; set; }

        public static InvestorHistoryModel Create(IInvestorHistoryItem item)
        {
            return new InvestorHistoryModel
            {
                Email = item.Email,
                WhenUtc = item.WhenUtc,
                Action = item.Action,
                Json = item.Json
            };
        }
    }

    public class InvestorEmailsResponse
    {
        public InvestorEmailModel[] Emails { get; set; }

        public static InvestorEmailsResponse Create(IEnumerable<IEmailHistoryItem> emails)
        {
            return new InvestorEmailsResponse { Emails = emails.Select(f => InvestorEmailModel.Create(f)).ToArray() };
        }
    }

    public class InvestorEmailModel
    {
        public string Email { get; set; }

        public DateTime WhenUtc { get; set; }

        public string Type { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public static InvestorEmailModel Create(IEmailHistoryItem item)
        {
            return new InvestorEmailModel
            {
                Email = item.Email,
                WhenUtc = item.WhenUtc,
                Type = item.Type,
                Subject = item.Subject,
                Body = item.Body
            };
        }
    }    
}
