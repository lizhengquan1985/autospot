using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    public class Test
    {
        public static void GoTest()
        {
            while (true)
            {
                Console.WriteLine("请选择进入测试的分支");
                var f = Console.ReadLine();
                if (f == "balance")
                {
                    Balance();
                }
                else if (f == "order")
                {
                    Order();
                }
                else if (f == "detail")
                {
                    AccountBalanceDetail();
                }
            }
        }

        public static void Balance()
        {
            var res = new AccountOrder().Accounts();
            Console.WriteLine(res);
            Console.WriteLine(res.data.Count);
            while (true)
            {
                Console.WriteLine("请输入 id：");
                var id = Console.ReadLine();
                var b = new AccountOrder().AccountBalance(id);
                b.data.list = b.data.list.Where(it => it.balance > 0).ToList();
                var usdt = b.data.list.Find(it => it.currency == "usdt");
                Console.WriteLine(JsonConvert.SerializeObject(b));
                Console.WriteLine(JsonConvert.SerializeObject(usdt));
            }

            //new CoinDao().InsertLog(new BuyRecord()
            //{
            //     BuyCoin ="ltc",
            //     BuyPrice = new decimal(1.1),
            //      BuyDate = DateTime.Now,
            //       HasSell = false,
            //});

            //var list = new CoinDao().ListNoSellRecord("ltc");
            //Console.WriteLine(list.Count);
            //new CoinDao().SetHasSell(1);

            //while (true)
            //{
            //    Console.WriteLine("请输入：");
            //    var coin = Console.ReadLine();
            //    ResponseOrder order = new AccountOrder().NewOrderBuy(AccountConfig.mainAccountId, 1, (decimal)0.01, null, coin, "usdt");
            //}

            //while (true)
            //{
            //    Console.WriteLine("请输入：");
            //    var coin = Console.ReadLine();

            //    decimal lastLow;
            //    decimal nowOpen;
            //    var flexPointList = new CoinAnalyze().Analyze(coin, "usdt", out lastLow, out nowOpen);
            //    foreach (var flexPoint in flexPointList)
            //    {
            //        Console.WriteLine($"{flexPoint.isHigh}, {flexPoint.open}, {Utils.GetDateById(flexPoint.id)}");
            //    }
            //}
        }

        public static void Order()
        {
            while (true)
            {
                Console.WriteLine("请输入 orderid：");
                var orderId = Console.ReadLine();
                string orderQuery = "";
                var b = new AccountOrder().QueryOrder(orderId, out orderQuery);
                Console.WriteLine(JsonConvert.SerializeObject(b));

                string orderDetail = "";
                var detail = new AccountOrder().QueryDetail(orderId, out orderDetail);
                Console.WriteLine(detail);
            }
        }

        public static void AccountBalanceDetail()
        {
            while (true)
            {
                Console.WriteLine("请输入 accountname：");
                var name = Console.ReadLine();
                AccountConfig.init(name);

                // 获取主账户的财富值
                var accountBalance = new AccountOrder().AccountBalance(AccountConfig.mainAccountId);
                //foreach (var item in accountBalance.data.list)
                //{
                //    Console.WriteLine($"{item.currency} -- {item.balance}");
                //}

                // 统计被套牢的数据
                Dictionary<string, decimal> coins = new Dictionary<string, decimal>();

                var noselllist = new CoinDao().ListAllNoSellRecord(AccountConfig.mainAccountId);
                foreach(var item in noselllist)
                {
                    if (coins.ContainsKey(item.Coin))
                    {
                        coins[item.Coin] += item.BuyTotalQuantity;
                    }
                    else
                    {
                        coins.Add(item.Coin, item.BuyTotalQuantity);
                    }
                }

                var noselllist2 = new CoinDao().ListNoSellRecordFromOther();
                foreach (var item in noselllist2)
                {
                    if (coins.ContainsKey(item.BuyCoin))
                    {
                        coins[item.BuyCoin] += item.BuyAmount;
                    }
                    else
                    {
                        coins.Add(item.BuyCoin, item.BuyAmount);
                    }
                }
                foreach (var item in accountBalance.data.list)
                {
                    if(item.balance == 0)
                    {
                        continue;
                    }
                    decimal tl = 0;
                    if (coins.ContainsKey(item.currency))
                    {
                        tl = coins[item.currency];
                    }
                    Console.WriteLine($"{item.currency} -- {item.balance} --》{tl}");
                }
            }
        }
    }
}
