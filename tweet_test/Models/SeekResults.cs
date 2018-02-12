using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tweet_test.Models
{
    public class SeekResults
    {
        public Tweet tweet { get; set; }
        public TwitterAccount account { get; set; }
    }

    public class AccountTweetPackage
    {
        public TwitterAccount account { get; set; }
        public Tweet[] tweetArray { get; set; }
    }

    public class Tweet
    {
        public String Id { get; set; }
        public String Text { get; set; }
        public double? Score { get; set; }
        public double? Percentage { get; set; }
    }

    public class Word
    {   
        public int Count { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public string Lemma { get; set; }
        public string TweetId { get; set; }
        public string Type { get; set; }
        public double? PosScore { get; set; }
        public double? NegScore { get; set; }
    }
}