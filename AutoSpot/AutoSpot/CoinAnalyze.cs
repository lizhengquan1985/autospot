using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    public class FlexPoint
    {
        public bool isHigh { get; set; }
        public decimal open { get; set; }
        public long id { get; set; }
    }
    /// <summary>
    /// 分析
    /// 1. 是否是拐点
    /// 2. 离上一次最高多少
    /// 3. 今日最高， 今日最低， 目前离最高多少，离最低多少
    /// 4. 24小时内最高， 24小时内最低，目前
    /// 4. 48小时内最高， 48小时内最低，目前
    /// 4. 72小时内最高， 72小时内最低，目前
    /// </summary>
    public class CoinAnalyze
    {
        static ILog logger = LogManager.GetLogger("CoinAnalyze");

        /// <summary>
        /// 分析价位走势
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="toCoin"></param>
        public List<FlexPoint> Analyze(string coin, string toCoin, out decimal lastLow, out decimal nowOpen)
        {
            nowOpen = 0;
            lastLow = 999999999;

            try
            {
                ResponseKline res = new AnaylyzeApi().kline(coin + toCoin, "1min", 1440);
                Console.WriteLine($"总数：{res.data.Count}");
                //Console.WriteLine(Utils.GetDateById(res.data[0].id));
                //Console.WriteLine(Utils.GetDateById(res.data[res.data.Count - 1].id));

                nowOpen = res.data[0].open;

                List<FlexPoint> flexPointList = new List<FlexPoint>();

                decimal openHigh = res.data[0].open;
                decimal openLow = res.data[0].open;
                long idHigh = res.data[0].id;
                long idLow = res.data[0].id;
                int lastHighOrLow = 0; // 1 high, -1: low
                foreach (var item in res.data)
                {
                    if (item.open > openHigh)
                    {
                        openHigh = item.open;
                        idHigh = item.id;
                    }
                    if (item.open < openLow)
                    {
                        openLow = item.open;
                        idLow = item.id;
                    }

                    if (openHigh >= openLow * (decimal)1.025)
                    {
                        var dtHigh = Utils.GetDateById(idHigh);
                        var dtLow = Utils.GetDateById(idLow);
                        // 相差了2%， 说明是一个节点了。
                        if (idHigh > idLow && lastHighOrLow != 1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = true, open = openHigh, id = idHigh });
                            lastHighOrLow = 1;
                            openHigh = openLow;
                            idHigh = idLow;
                        }
                        else if (idHigh < idLow && lastHighOrLow != -1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = false, open = openLow, id = idLow });
                            lastHighOrLow = -1;
                            openLow = openHigh;
                            idLow = idHigh;
                        }
                        else if (lastHighOrLow == 1)
                        {

                        }
                    }
                }

                if (flexPointList[0].isHigh)
                {
                    // 
                    foreach (var item in res.data)
                    {
                        if (item.id < flexPointList[0].id && lastLow > item.open)
                        {
                            lastLow = item.open;
                        }
                    }
                }

                if (flexPointList.Count < 0)
                {
                    logger.Error($"--------------{idHigh}------{idLow}------------------");
                    logger.Error(JsonConvert.SerializeObject(flexPointList));
                    logger.Error(JsonConvert.SerializeObject(res.data));
                }

                return flexPointList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            return new List<FlexPoint>();
        }

        public decimal AnalyzeNeedSell(decimal comparePrice, DateTime compareDate, string coin, string toCoin, out decimal nowOpen)
        {
            // 当前open
            nowOpen = 0;

            decimal higher = new decimal(0);

            try
            {
                ResponseKline res = new AnaylyzeApi().kline(coin + toCoin, "1min", 1440);

                nowOpen = res.data[0].open;

                List<FlexPoint> flexPointList = new List<FlexPoint>();

                decimal openHigh = res.data[0].open;
                decimal openLow = res.data[0].open;
                long idHigh = 0;
                long idLow = 0;
                int lastHighOrLow = 0; // 1 high, -1: low
                foreach (var item in res.data)
                {
                    if (Utils.GetDateById(item.id) < compareDate)
                    {
                        continue;
                    }

                    if (item.open > higher)
                    {
                        higher = item.open;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("1111111111111111111111 over");
            }
            return higher;
        }
    }
}
