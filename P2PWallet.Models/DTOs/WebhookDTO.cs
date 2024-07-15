using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    using System;
    using System.Collections.Generic;

    public class WebhookDTO
    {
        public string Event { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public string Id { get; set; }
        public string Domain { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public string Amount { get; set; }
        public string GatewayResponse { get; set; }
        public DateTime PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Channel { get; set; }
        public string Currency { get; set; }
        public string IpAddress { get; set; }
        public string Metadata { get; set; }
        public WebhookLog Log { get; set; }
        public object Fees { get; set; }
        public Customer Customer { get; set; }
        public Authorization Authorization { get; set; }
        public object Plan { get; set; }
    }

    public class WebhookLog
    {
        public string TimeSpent { get; set; }
        public string Attempts { get; set; }
        public string Authentication { get; set; }
        public string Errors { get; set; }
        public bool Success { get; set; }
        public bool Mobile { get; set; }
        public List<object> Input { get; set; }
        public string Channel { get; set; }
        public List<LogHistory> History { get; set; }
    }

    public class LogHistory
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CustomerCode { get; set; }
        public object Phone { get; set; }
        public object Metadata { get; set; }
        public string RiskAction { get; set; }
    }

    public class Authorization
    {
        public string AuthorizationCode { get; set; }
        public string Bin { get; set; }
        public string Last4 { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string CardType { get; set; }
        public string Bank { get; set; }
        public string CountryCode { get; set; }
        public string Brand { get; set; }
        public string AccountName { get; set; }
    }

}
