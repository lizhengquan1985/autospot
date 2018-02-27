using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    public class ResponseKline
    {
        public string status { get; set; }
        public string ch { get; set; }
        public string ts { get; set; }
        public List<KlineData> data { get; set; }
    }

    public class KlineData
    {
        public long id { get; set; }
        public decimal amount { get; set; }
        public decimal count { get; set; }
        public decimal open { get; set; }
        public decimal close { get; set; }
        public decimal low { get; set; }
        public decimal high { get; set; }
        public decimal vol { get; set; }
    }

    public class ResponseAccount
    {
        public string status { get; set; }
        public List<AccountData> data { get; set; }
    }

    public class AccountData
    {
        public string id { get; set; }
        public string type { get; set; }
        public string subtype { get; set; }
    }

    public class ResponseOrder
    {
        public string status { get; set; }
        public string data { get; set; }
    }

    public class AccountBalance
    {
        public string status { get; set; }
        public AccountBalanceData data { get; set; }
    }

    public class AccountBalanceData
    {
        public long id { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public List<AccountBalanceItem> list { get; set; }
    }

    public class AccountBalanceItem
    {
        public string currency { get; set; }
        public decimal balance { get; set; }
        public string type { get; set; }
    }
}
