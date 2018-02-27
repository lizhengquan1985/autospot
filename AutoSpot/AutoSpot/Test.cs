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
    }
}
