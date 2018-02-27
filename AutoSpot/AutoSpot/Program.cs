using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger("program");
            logger.Error("-------------------------- 软件启动 ---------------------------------");

            AccountConfig.init("yxq");

            Console.WriteLine($"{AccountConfig.mainAccountId}， {AccountConfig.accessKey}， {AccountConfig.secretKey}， {AccountConfig.sqlConfig}");

            Console.Read();
            //Test();


            Run();
        }

        public static void Test()
        {
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

            //var res = new AccountOrder().Accounts();
            //Console.WriteLine(res);
            //Console.WriteLine(res.data.Count);
            //while (true)
            //{
            //    Console.WriteLine("请输入 id：");
            //    var id = Console.ReadLine();
            //    var b = new AccountOrder().AccountBalance(id);
            //    b.data.list = b.data.list.Where(it => it.balance > 0).ToList();
            //    var usdt = b.data.list.Find(it => it.currency == "usdt");
            //    Console.WriteLine(JsonConvert.SerializeObject(b));
            //    Console.WriteLine(JsonConvert.SerializeObject(usdt));
            //}

            //while (true)
            //{
            //    Console.WriteLine("请输入：");
            //    var coin = Console.ReadLine();
            //    ResponseOrder order = new AccountOrder().NewOrderBuy(AccountConfig.mainAccountId, 1, (decimal)0.01, null, coin, "usdt");
            //}

            while (true)
            {
                Console.WriteLine("请输入：");
                var coin = Console.ReadLine();

                decimal lastLow;
                decimal nowOpen;
                var flexPointList = new CoinAnalyze().Analyze(coin, "usdt", out lastLow, out nowOpen);
                foreach (var flexPoint in flexPointList)
                {
                    Console.WriteLine($"{flexPoint.isHigh}, {flexPoint.open}, {Utils.GetDateById(flexPoint.id)}");
                }
            }
        }

        public static void Run()
        {
            while (true)
            {

                try
                {
                    //BTC.Do();
                    BCH.Do();
                    ETH.Do();
                    ETC.Do();
                    LTC.Do();

                    EOS.Do();
                    XRP.Do();
                    OMG.Do();
                    DASH.Do();
                    ZEC.Do();
                    Thread.Sleep(1000 * 5);

                    // 创新
                    ITC.Do();
                    NAS.Do();
                    RUFF.Do();
                    ZIL.Do();
                    DTA.Do();
                    Thread.Sleep(1000 * 5);

                    LET.Do();
                    HT.Do();
                    THETA.Do();
                    HSR.Do();
                    QTUM.Do();
                    Thread.Sleep(1000 * 5);

                    SNT.Do();
                    IOST.Do();
                    NEO.Do();
                    STORJ.Do();
                    GNT.Do();
                    Thread.Sleep(1000 * 5);

                    CVC.Do();
                    SMT.Do();
                    VEN.Do();
                    ELF.Do();
                    XEM.Do();
                    Thread.Sleep(1000 * 5);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
