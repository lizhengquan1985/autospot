using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    /// <summary>
    /// 如果已存交易量recordcount少，则减少更多才购入。 如果交易良多，则少也购入。 同下。
    /// 如果交易量少，则购入少一些。 如果多。则购入多一些。，--》 30-n 计算最近一段时间的价格回落排行。 来动态决定投资方向
    /// 计算频率加大?????????????????。
    /// 每沉淀一个，多加0.5% 最高不超过10%
    /// 稳定值得计算， 越稳定，越快速出手??????????????
    /// </summary>
    public class CoinTrade
    {
        static ILog logger = LogManager.GetLogger(typeof(CoinTrade));
        static int i = 0;

        private static AccountBalanceItem usdt;
        private static int noSellCount = -1;

        public static bool CheckBalance()
        {
            i++;
            if (usdt == null)
            {
                var accountId = AccountConfig.mainAccountId;
                var accountInfo = new AccountOrder().AccountBalance(accountId);
                usdt = accountInfo.data.list.Find(it => it.currency == "usdt");
            }

            if (usdt.balance < 10 && i % 100 == 0)
            {
                Console.WriteLine($"--------------------- 余额{usdt.balance}----------------------------");
            }

            if (usdt.balance < 6)
            {
                Console.WriteLine("---------------------余额小于6，无法交易----------------------------");
                return false;
            }
            return true;
        }

        public static decimal GetRecommendBuyAmount(string coin)
        {
            if (noSellCount < 0)
            {
                noSellCount = new CoinDao().GetAllNoSellRecordCount();
            }

            if (usdt == null)
            {
                var accountId = AccountConfig.mainAccountId;
                var accountInfo = new AccountOrder().AccountBalance(accountId);
                usdt = accountInfo.data.list.Find(it => it.currency == "usdt");
            }

            var calcPencert = getCalcPencent(new CoinAnalyze().CalcPercent(coin));

            if (noSellCount < 80)
            {
                return (usdt.balance / 80) / calcPencert;///  0.8,  1,  1.2,  1.5;
            }

            return (usdt.balance / 30) / calcPencert;///  0.8,  1,  1.2,  1.5;

            //if (noSellCount > 80)
            //{
            //    return usdt.balance / 30;
            //}

            //// 让每个承受8轮
            //return usdt.balance / (100 - noSellCount);
        }

        private static decimal getCalcPencent(CalcPriceHuiluo huiluo)
        {
            if (huiluo == CalcPriceHuiluo.high)
            {
                return (decimal)1;
            }
            if (huiluo == CalcPriceHuiluo.highest)
            {
                return (decimal)0.8;
            }
            if (huiluo == CalcPriceHuiluo.little)
            {
                return (decimal)1.2;
            }
            return (decimal)1.5;
        }

        public static void ClearData()
        {
            usdt = null;
            noSellCount = -1;
        }

        public static bool CheckCanBuy(decimal nowOpen, decimal nearLowOpen)
        {
            //nowOpen > flexPointList[0].open * (decimal)1.005 && nowOpen < flexPointList[0].open * (decimal)1.01
            return nowOpen > nearLowOpen * (decimal)1.005 && nowOpen < nearLowOpen * (decimal)1.01;
        }

        public static bool CheckCanSell(decimal buyPrice, decimal nearHigherOpen, decimal nowOpen, decimal gaoyuPercentSell = (decimal)1.03)
        {
            //item.BuyPrice, higher, itemNowOpen
            // if (item.BuyPrice * (decimal)1.05 < higher && itemNowOpen * (decimal)1.005 < higher)
            if (nowOpen < buyPrice * gaoyuPercentSell)
            {
                // 如果不高于 3% 没有意义
                return false;
            }

            if (nowOpen * (decimal)1.005 < nearHigherOpen)
            {
                // 表示回头趋势， 暂时定为 0.5% 就有回头趋势
                return true;
            }

            return false;
        }

        public static void Start(string coin)
        {
            try
            {
                BusinessRun(coin);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        public static bool IsQuickRise(ResponseKline res)
        {
            // 判断是否快速上涨，如果是快速上涨，防止追涨
            var klineData = res.data;
            // 暂时判断 1个小时内是否上涨超过12%， 如果超过，则控制下
            var max = (decimal)0;
            var min = (decimal)9999999;
            var nowOpen = klineData[0].open;
            for(var i=0; i< 60; i++)
            {
                var item = klineData[i];
                if(max < item.open)
                {
                    max = item.open;
                }
                if(min > item.open)
                {
                    min = item.open;
                }
            }
            bool isQuickRise = false;
            if(max > min * (decimal)1.12)
            {
                if(nowOpen > min * (decimal)1.03)
                {
                    logger.Error("一个小时内有大量的上涨，防止追涨，所以不能交易。");
                    isQuickRise = true;
                }
            }
            return isQuickRise;
        }

        public static void BusinessRun(string coin)
        {
            var accountId = AccountConfig.mainAccountId;
            ResponseKline res = new AnaylyzeApi().kline(coin + "usdt", "1min", 1440);
            // 获取最近行情
            decimal lastLow;
            decimal nowOpen;
            // 分析是否下跌， 下跌超过一定数据，可以考虑
            var flexPointList = new CoinAnalyze().Analyze(res, out lastLow, out nowOpen);
            if (flexPointList.Count == 0)
            {
                logger.Error($"--------------> 分析结果数量为0 {coin}");
                return;
            }

            decimal recommendAmount = GetRecommendBuyAmount(coin);
            Console.Write($"spot--------> 开始 {coin}  推荐额度：{decimal.Round(recommendAmount, 2)} ");

            try
            {
                // 查询出结果还没好的数据， 去搜索一下
                var noSetBuySuccess = new CoinDao().ListNotSetBuySuccess(accountId, coin);
                foreach (var item in noSetBuySuccess)
                {
                    QueryDetailAndUpdate(item.BuyOrderId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            try
            {
                // 查询出结果还没好的数据， 去搜索一下
                var noSetSellSuccess = new CoinDao().ListHasSellNotSetSellSuccess(accountId, coin);
                foreach (var item in noSetSellSuccess)
                {
                    Console.WriteLine("----------> " + JsonConvert.SerializeObject(item));
                    QuerySellDetailAndUpdate(item.SellOrderId);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            if (!flexPointList[0].isHigh && CheckBalance() && recommendAmount > (decimal)0.3 && !IsQuickRise(res))
            {
                var noSellCount = new CoinDao().GetNoSellRecordCount(accountId, coin);
                // 最后一次是高位, 没有交易记录， 则判断是否少于最近的6%
                if (noSellCount <= 0 && CheckCanBuy(nowOpen, flexPointList[0].open) && new CoinAnalyze().CheckCalcMaxhuoluo(coin, "usdt", "5min"))
                {
                    // 可以考虑
                    decimal buyQuantity = recommendAmount / nowOpen;
                    buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                    decimal orderPrice = decimal.Round(nowOpen * (decimal)1.005, getPrecisionNumber(coin));
                    ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, coin, "usdt");
                    if (order.status != "error")
                    {
                        new CoinDao().CreateSpotRecord(new SpotRecord()
                        {
                            Coin = coin,
                            UserName = AccountConfig.userName,
                            BuyTotalQuantity = buyQuantity,
                            BuyOrderPrice = orderPrice,
                            BuyDate = DateTime.Now,
                            HasSell = false,
                            BuyOrderResult = JsonConvert.SerializeObject(order),
                            BuyAnalyze = JsonConvert.SerializeObject(flexPointList),
                            AccountId = accountId,
                            BuySuccess = false,
                            BuyTradePrice = 0,
                            BuyOrderId = order.data,
                            BuyOrderQuery = "",
                            SellAnalyze = "",
                            SellOrderId = "",
                            SellOrderQuery = "",
                            SellOrderResult = ""
                        });
                        ClearData();
                        // 下单成功马上去查一次
                        QueryDetailAndUpdate(order.data);
                    }
                    else
                    {
                        logger.Error($"下单结果 coin{coin} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowOpen} {JsonConvert.SerializeObject(order)}");
                        logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                    }
                }

                if (noSellCount > 0)
                {
                    // 获取最小的那个， 如果有，
                    decimal minBuyPrice = 9999;
                    var noSellList = new CoinDao().ListNoSellRecord(accountId, coin);
                    foreach (var item in noSellList)
                    {
                        if (item.BuyOrderPrice < minBuyPrice)
                        {
                            minBuyPrice = item.BuyOrderPrice;
                        }
                    }

                    // 再少于5%， 
                    var per = new CoinAnalyze().CalcPercent(coin);
                    decimal pecent = getCalcPencent222(per);//noSellCount >= 15 ? (decimal)1.03 : (decimal)1.025;
                    if (nowOpen * pecent < minBuyPrice)
                    {
                        decimal buyQuantity = recommendAmount / nowOpen;
                        buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                        decimal orderPrice = decimal.Round(nowOpen * (decimal)1.005, getPrecisionNumber(coin));
                        ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, coin, "usdt");
                        if (order.status != "error")
                        {
                            new CoinDao().CreateSpotRecord(new SpotRecord()
                            {
                                Coin = coin,
                                UserName = AccountConfig.userName,
                                BuyTotalQuantity = buyQuantity,
                                BuyOrderPrice = orderPrice,
                                BuyDate = DateTime.Now,
                                HasSell = false,
                                BuyOrderResult = JsonConvert.SerializeObject(order),
                                BuyAnalyze = JsonConvert.SerializeObject(flexPointList),
                                AccountId = accountId,
                                BuySuccess = false,
                                BuyTradePrice = 0,
                                BuyOrderId = order.data,
                                BuyOrderQuery = "",
                                SellAnalyze = "",
                                SellOrderId = "",
                                SellOrderQuery = "",
                                SellOrderResult = ""
                            });
                            ClearData();
                            // 下单成功马上去查一次
                            QueryDetailAndUpdate(order.data);
                        }
                        else
                        {
                            logger.Error($"下单结果 coin{coin} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowOpen} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                    }
                }
            }

            // 查询数据库中已经下单数据，如果有，则比较之后的最高值，如果有，则出售
            var needSellList = new CoinDao().ListBuySuccessAndNoSellRecord(accountId, coin);
            foreach (var item in needSellList)
            {
                // 分析是否 大于
                decimal itemNowOpen = 0;
                decimal higher = new CoinAnalyze().AnalyzeNeedSell(item.BuyOrderPrice, item.BuyDate, coin, "usdt", out itemNowOpen);

                decimal gaoyuPercentSell = (decimal)1.03;
                if (needSellList.Count > 10)
                {
                    gaoyuPercentSell = (decimal)1.050;
                }
                else if (needSellList.Count > 9)
                {
                    gaoyuPercentSell = (decimal)1.048;
                }
                else if (needSellList.Count > 8)
                {
                    gaoyuPercentSell = (decimal)1.046;
                }
                else if (needSellList.Count > 7)
                {
                    gaoyuPercentSell = (decimal)1.044;
                }
                else if (needSellList.Count > 6)
                {
                    gaoyuPercentSell = (decimal)1.042;
                }
                else if (needSellList.Count > 5)
                {
                    gaoyuPercentSell = (decimal)1.04;
                }

                if (CheckCanSell(item.BuyOrderPrice, higher, itemNowOpen, gaoyuPercentSell))
                {
                    decimal sellQuantity = item.BuyTotalQuantity * (decimal)0.99;
                    sellQuantity = decimal.Round(sellQuantity, getSellPrecisionNumber(coin));
                    // 出售
                    decimal sellPrice = decimal.Round(itemNowOpen * (decimal)0.985, getPrecisionNumber(coin));
                    ResponseOrder order = new AccountOrder().NewOrderSell(accountId, sellQuantity, sellPrice, null, coin, "usdt");
                    if (order.status != "error")
                    {
                        new CoinDao().ChangeDataWhenSell(item.Id, sellQuantity, sellPrice, JsonConvert.SerializeObject(order), JsonConvert.SerializeObject(flexPointList), order.data);
                        // 下单成功马上去查一次
                        QuerySellDetailAndUpdate(order.data);
                    }
                    else
                    {
                        logger.Error($"出售结果 coin{coin} accountId:{accountId}  出售数量{sellQuantity} itemNowOpen{itemNowOpen} higher{higher} {JsonConvert.SerializeObject(order)}");
                        logger.Error($"出售结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                    }
                    ClearData();
                }
            }
        }

        private static decimal getCalcPencent222(CalcPriceHuiluo huiluo)
        {
            if (huiluo == CalcPriceHuiluo.highest)
            {
                return (decimal)1.026;
            }
            if (huiluo == CalcPriceHuiluo.high)
            {
                return (decimal)1.028;
            }
            
            if (huiluo == CalcPriceHuiluo.little)
            {
                return (decimal)1.030;
            }
            return (decimal)1.032;
        }

        private static void QueryDetailAndUpdate(string orderId)
        {
            string orderQuery = "";
            var queryOrder = new AccountOrder().QueryOrder(orderId, out orderQuery);
            if (queryOrder.status == "ok" && queryOrder.data.state == "filled")
            {
                string orderDetail = "";
                var detail = new AccountOrder().QueryDetail(orderId, out orderDetail);
                decimal maxPrice = 0;
                foreach (var item in detail.data)
                {
                    if (maxPrice < item.price)
                    {
                        maxPrice = item.price;
                    }
                }
                if (detail.status == "ok")
                {
                    new CoinDao().UpdateTradeRecordBuySuccess(orderId, maxPrice, orderQuery);
                }
            }
        }

        private static void QuerySellDetailAndUpdate(string orderId)
        {
            string orderQuery = "";
            var queryOrder = new AccountOrder().QueryOrder(orderId, out orderQuery);
            if (queryOrder.status == "ok" && queryOrder.data.state == "filled")
            {
                string orderDetail = "";
                var detail = new AccountOrder().QueryDetail(orderId, out orderDetail);
                decimal minPrice = 99999999;
                foreach (var item in detail.data)
                {
                    if (minPrice > item.price)
                    {
                        minPrice = item.price;
                    }
                }
                // 完成
                new CoinDao().UpdateTradeRecordSellSuccess(orderId, minPrice, orderQuery);
            }
        }

        public static int getPrecisionNumber(string coin)
        {
            if (coin == "btc" || coin == "bch" || coin == "eth" || coin == "etc" || coin == "ltc" || coin == "eos" || coin == "omg" || coin == "dash" || coin == "zec" || coin == "hsr"
                 || coin == "qtum" || coin == "neo" || coin == "ven" || coin == "nas")
            {
                return 2;
            }
            return 4;
        }

        public static int getSellPrecisionNumber(string coin)
        {
            if (coin == "cvc" || coin == "ht" || coin == "xrp" || coin == "ela" || coin == "mds" || coin == "trx")
            {
                return 2;
            }
            return 4;
        }

        /// <summary>
        /// 获取购买数量的精度
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public static int GetBuyQuantityPrecisionNumber(string coin)
        {
            if (coin == "btc")
            {
                return 4;
            }

            if (coin == "bch" || coin == "dash" || coin == "eth" || coin == "zec" || coin == "neo")
            {
                return 3;
            }

            return 2;
        }
    }
}
