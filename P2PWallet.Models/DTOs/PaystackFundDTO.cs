using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class PaystackFundDTO
    {
        /*
               * Amount
               * Email
               * Reference
               * currency
        */
        public string amount { get; set; }
        public string reference { get; set; } = string.Empty;
        public string currency { get; set; } = "NGN";
        public string email { get; set; } = string.Empty;
    }
    public class PaystackViewDTO
    {
        public decimal Fund { get; set; }
    }
    public class PaystackResponseDTO
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }
    public class Data
    {
        public string authorization_url { get; set; }
        public string access_code { get; set; }
        public string reference { get; set; }
    }
   


}
