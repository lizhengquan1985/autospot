using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSpot
{
    public class AccountConfig
    {
        public static string userName = "";
        // default yanxiuq
        public static string accessKey = "";
        public static string secretKey = "";

        public static string mainAccountId = "";

        public static string sqlConfig = "server=localhost;port=3306;user id=root; password=lyx123456; database=studyplan; pooling=true; charset=utf8mb4";

        public static void init(string role)
        {
            userName = role;

            // 初始化accessKey，secretKey
            if (role == "lzq")
            {
            }
            else if (role == "yxq")
            {
            }
        }
    }
}
