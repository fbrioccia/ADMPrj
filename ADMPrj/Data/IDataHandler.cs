using ADMPrj.Models;
using System;
using System.Collections.Generic;

namespace ADMPrj.Data
{
    public interface INeo4JDataHandler
    {
        TweetResults[] TwitterAccountList();
        void SaveAccountTweets(SeekResults[] seekResults);
        Tuple<double, double> ComputeScore(String tweetId);
        Tuple<double,double> ComputePatternScore(String tweetId); //<score,percentage>
    }
}
