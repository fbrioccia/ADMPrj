using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tweet_test.Models
{
    public class TweetResults
    {
        public TwitterAccount TweetAccount { get; set; }
        public Tweet[] Tweet { get; set; }
    }
}