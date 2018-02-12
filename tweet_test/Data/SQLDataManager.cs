using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using tweet_test.Models;
using tweet_test.Utilities;

namespace tweet_test.Data
{
    public class SQLDataManager
    {
        public string ConnectionString = $"Data Source={HttpContext.Current.Server.MapPath("/bin")}/Lex.db;Version=3;";

        public Score GetWordScore(string word)
        {
            using (var dbConn = new SQLiteConnection(ConnectionString))
            {
                dbConn.Open();

                var sqlCmd = new SQLiteCommand($"SELECT PosScore, NegScore from Lex Where word = '{word}' limit 1", dbConn);

                SQLiteDataReader sqlDataRead = sqlCmd.ExecuteReader();

                if (!sqlDataRead.HasRows)
                    return new Score() { Positive = 0, Negative = 0 };

                sqlDataRead.Read();
                return new Score() { Positive = Convert.ToDouble(sqlDataRead.GetValue(0)), Negative = Convert.ToDouble(sqlDataRead.GetValue(1)) };
            }
        }
    }
}