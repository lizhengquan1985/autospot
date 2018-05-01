using MySql.Data.MySqlClient;
using SharpDapper;
using SharpDapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    public class CoinDao
    {
        public CoinDao()
        {
            string connectionString = AccountConfig.sqlConfig;
            var connection = new MySqlConnection(connectionString);
            Database = new DapperConnection(connection);

        }
        protected IDapperConnection Database { get; private set; }

        public void CreateSpotRecord(SpotRecord spotRecord)
        {
            if (spotRecord.BuyAnalyze.Length > 4500)
            {
                spotRecord.BuyAnalyze = spotRecord.BuyAnalyze.Substring(0, 4500);
            }
            if (spotRecord.BuyOrderResult.Length > 500)
            {
                spotRecord.BuyOrderResult = spotRecord.BuyOrderResult.Substring(0, 500);
            }

            using (var tx = Database.BeginTransaction())
            {
                Database.Insert(spotRecord);
                tx.Commit();
            }
        }

        public void UpdateTradeRecordBuySuccess(string buyOrderId, decimal buyTradePrice, string buyOrderQuery)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set BuyOrderQuery='{buyOrderQuery}', BuySuccess=1 , BuyTradePrice={buyTradePrice} where BuyOrderId ='{buyOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        public List<SpotRecord> ListNotSetBuySuccess(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and BuySuccess=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<SpotRecord>(sql).ToList();
        }

        public List<SpotRecord> ListHasSellNotSetSellSuccess(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and SellSuccess=0 and HasSell=1 and UserName='{AccountConfig.userName}'";
            return Database.Query<SpotRecord>(sql).ToList();
        }

        /// <summary>
        /// 获取没有出售的数量
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="coin"></param>
        /// <returns></returns>
        public int GetNoSellRecordCount(string accountId, string coin)
        {
            var sql = $"select count(1) from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and UserName='{AccountConfig.userName}' and celuo=0";
            return Database.Query<int>(sql).FirstOrDefault();
        }

        public int GetNoSellRecordCountForCelue1(string accountId, string coin)
        {
            var sql = $"select count(1) from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and UserName='{AccountConfig.userName}' and celuo=1 and BuyDate>'{DateTime.Now.AddHours(-6).ToString("yy-MM-dd HH:mm:ss")}'";
            return Database.Query<int>(sql).FirstOrDefault();
        }

        public List<SpotRecord> ListNoSellRecord(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and UserName='{AccountConfig.userName}' and celuo=0";
            return Database.Query<SpotRecord>(sql).ToList();
        }

        public List<SpotRecord> ListAllNoSellRecord(string accountId)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and HasSell=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<SpotRecord>(sql).ToList();
        }

        public List<BuyRecord> ListNoSellRecordFromOther()
        {
            var sql = $"select * from t_buy_record where HasSell=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<BuyRecord>(sql).ToList();
        }

        public List<SpotRecord> ListBuySuccessAndNoSellRecord(string accountId, string coin,int celue)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and BuySuccess=1 and UserName='{AccountConfig.userName}' and celuo={celue}";
            return Database.Query<SpotRecord>(sql).ToList();
        }

        public int GetAllNoSellRecordCount()
        {
            var sql = $"select count(1) from t_spot_record where HasSell=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<int>(sql).FirstOrDefault();
        }

        public void ChangeDataWhenSell(long id, decimal sellTotalQuantity, decimal sellOrderPrice, string sellOrderResult, string sellAnalyze, string sellOrderId)
        {
            if (sellAnalyze.Length > 4500)
            {
                sellAnalyze = sellAnalyze.Substring(0, 4500);
            }
            if (sellOrderResult.Length > 500)
            {
                sellOrderResult = sellOrderResult.Substring(0, 500);
            }

            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set HasSell=1, SellTotalQuantity={sellTotalQuantity}, sellOrderPrice={sellOrderPrice}, SellDate=now(), SellAnalyze='{sellAnalyze}', SellOrderResult='{sellOrderResult}',SellOrderId={sellOrderId} where Id = {id}";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        public void UpdateTradeRecordSellSuccess(string sellOrderId, decimal sellTradePrice, string sellOrderQuery)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set SellOrderQuery='{sellOrderQuery}', SellSuccess=1 , SellTradePrice={sellTradePrice} where SellOrderId ='{sellOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }
    }

    [Table("t_spot_record")]
    public class SpotRecord
    {
        public long Id { get; set; }
        public string Coin { get; set; }
        public string AccountId { get; set; }
        public bool HasSell { get; set; }
        public string UserName { get; set; }


        public decimal BuyTotalQuantity { get; set; }
        public decimal BuyOrderPrice { get; set; }
        public decimal BuyTradePrice { get; set; }
        public DateTime BuyDate { get; set; }
        public string BuyOrderResult { get; set; }
        public bool BuySuccess { get; set; }


        public decimal SellTotalQuantity { get; set; }
        public decimal SellOrderPrice { get; set; }
        public decimal SellTradePrice { get; set; }
        public DateTime SellDate { get; set; }
        public string SellOrderResult { get; set; }
        public bool SellSuccess { get; set; }

        public string BuyAnalyze { get; set; }
        public string SellAnalyze { get; set; }

        public string BuyOrderId { get; set; }
        public string BuyOrderQuery { get; set; }
        public string SellOrderId { get; set; }
        public string SellOrderQuery { get; set; }
        public int Celuo { get; set; }
    }
}
