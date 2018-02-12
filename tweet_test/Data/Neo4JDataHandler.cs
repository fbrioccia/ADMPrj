using edu.stanford.nlp.tagger.maxent;
using Neo4j.Driver.V1;
using Neo4jClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using tweet_test.Models;
using tweet_test.Utilities;

namespace tweet_test.Data
{
    public class Neo4JDataHandler : INeo4JDataHandler
    {
        private const double weightPlus = 2.0;
        private const double weightMinus = 0.5;
 
        public TweetResults[] TwitterAccountList()
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();
                    //sion.Run("MATCH (n:Account) MATCH (t:Tweet) where (n)-[:PUBLISHED]->(t) RETURN n,t");
                    var graphNodes = client.Cypher
                        .Match("(n:Account) MATCH (t:Tweet)")
                        .Where("(n)-[:PUBLISHED]->(t)")
                        .Return((n, t) => new SeekResults
                        {
                            account = n.As<TwitterAccount>(),
                            tweet = t.As<Tweet>()
                        })
                        .Results;

                    var tweetAccountList = graphNodes.GroupBy(x => x.account).ToList();

                    Dictionary<TwitterAccount, List<SeekResults>> accountSeekResultDict = (from el in graphNodes
                                                                                           group el by el.account.Id
                                                                                           into groupedPack
                                                                                           select groupedPack).ToDictionary(gdc => gdc.ToList().First().account, gdc => gdc.ToList());

                    var accountTweetDict = new Dictionary<TwitterAccount, List<Tweet>>();
                    accountSeekResultDict.ToList().ForEach(x => accountTweetDict.Add(x.Key, new List<Tweet>()));
                    accountSeekResultDict.ToList().ForEach(x => x.Value.ToList().ForEach(y => accountTweetDict[x.Key].Add(y.tweet)));

                    List<TweetResults> arrayTweetRestult = new List<TweetResults>();
                    accountTweetDict.ToList().ForEach(d => arrayTweetRestult.Add(new TweetResults
                    {
                        TweetAccount = d.Key,
                        Tweet = d.Value.ToArray()
                    }));
                    return arrayTweetRestult.ToArray();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        public void SaveAccountTweets(SeekResults[] seekResults)
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();

                    for (int i = 0; i < seekResults.Length; i++)
                    {
                        //"MERGE (a:Account{ Id : '0051', Name : 'Mario Rossi',ImageUrl : 'https://pbs.twimg.com/profile_images/954696682201141248/sQa-k6hD_400x400.jpg'}) MERGE (t:Tweet{Id:'0001', Text:'Testo del Tweet', CreationDate: '10/02/2018'})  MERGE (a)-[:PUBLISHED]->(t)"
                        client.Cypher
                        .Merge($"(a:Account{{ Id : '{seekResults[i].account.Id}', Name : '{seekResults[i].account.Name}',ImageUrl : '{seekResults[i].account.ImageUrl}'}})")
                        .OnCreate()
                        .Set($"a.Id = '{seekResults[i].account.Id}', a.Name = '{seekResults[i].account.Name}',a.ImageUrl ='{seekResults[i].account.ImageUrl}'")
                        .Merge($"(t:Tweet{{Id:'{seekResults[i].tweet.Id}', Text:'{seekResults[i].tweet.Text.Replace("'", "’")}'}})")
                        .OnCreate()
                        .Set($"t.Id = '{seekResults[i].tweet.Id}', t.Text = '{seekResults[i].tweet.Text.Replace("'", "’")}'")
                        .Merge("(a)-[:PUBLISHED]->(t)")
                        .ExecuteWithoutResults();
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }

        }

        public Tuple<double, double> ComputeScore(String TweetId)
        {
            var tweet = GetTweet(TweetId);
            tweet.Text = Utils.Purify(tweet.Text);
            //var langController = new LanguageController();
            //langController.ParseText(tweet.Text);
            string model = @"/wsj-0-18-bidirectional-nodistsim.tagger";
            char[] delimiterChars = { ' ', '.', ':', '\t' };

            var dataManager = new SQLDataManager();

            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("/bin") + model))
                throw new Exception($"Check path to the model file '{model}'");

            // Loading POS Tagger
            var tagger = new MaxentTagger(HttpContext.Current.Server.MapPath("/bin") + model);

            var wordScoreDictionary = new Dictionary<string, Score>();

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();

                    var creationCmd = new List<String>();
                    var tokenArray = tweet.Text.ToLower().Split(delimiterChars).Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    var dictionaryTokenType = new List<Tuple<string, string>>();
                    for (int i = 0; i < tokenArray.Length - 1; i++)
                    {
                        var taggedTokenArray = tagger.tagString(tokenArray[i]).Split(new Char[] { '_', ' ' }).Where(x=>!String.IsNullOrEmpty(x)).ToArray();
                        for (int j = 0; j < taggedTokenArray.Length; j +=2)
                        {

                            dictionaryTokenType.Add(new Tuple<string,string>( taggedTokenArray[j].Replace("'", "’"), taggedTokenArray[j + 1].Replace("'", "’")));
                        }
                    }

                    for (int i = 0; i < dictionaryTokenType.Count-1; i++)
                    {
                        var taggedToken = dictionaryTokenType[i].Item1;
                        var firstTokenType = dictionaryTokenType[i].Item2;
                        var nextToken = dictionaryTokenType[i + 1].Item1;
                        var secondTokenType = dictionaryTokenType[i + 1].Item2;

                        if (Constants.PartOfSpeech.IsAdjective(firstTokenType))
                        {
                            if (!wordScoreDictionary.ContainsKey(taggedToken))
                                wordScoreDictionary[taggedToken] = dataManager.GetWordScore(taggedToken);
                        }

                        if (Constants.PartOfSpeech.IsAdjective(secondTokenType))
                        {
                            if (!wordScoreDictionary.ContainsKey(nextToken))
                                wordScoreDictionary[nextToken] = dataManager.GetWordScore(nextToken);
                        }

                        client.Cypher
                            .Merge($" (n:Word {{Lemma: '{taggedToken}', Type: '{firstTokenType}', IsFirst: '{tokenArray.First() == taggedToken}', IsLast: '{tokenArray.Last() == taggedToken}', TweetId : '{tweet.Id}' }})")
                            .OnCreate()
                            .Set($"n.Count = 1, n.Id = {i}")
                            .Merge($" (m:Word {{Lemma: '{nextToken}', Type: '{secondTokenType}', IsFirst: '{tokenArray.First() == nextToken}', IsLast: '{tokenArray.Last() == nextToken}', TweetId : '{tweet.Id}' }})")
                            .OnCreate()
                            .Set($"m.Count = 1, m.Id = {i + 1}")
                            .OnMatch()
                            .Set("m.Count = m.Count + 1")
                            .Merge("(n)-[r:NEXT]->(m)")
                            .OnCreate()
                            .Set("r.Count = 1")
                            .OnMatch()
                            .Set("r.Count = r.Count+1")
                            .Merge("(m)<-[t:PREV]-(n)")
                            .OnCreate()
                            .Set("t.Count = 1")
                            .OnMatch()
                            .Set("t.Count = t.Count+1")
                            .ExecuteWithoutResults();
                    }

                    foreach (var diEl in wordScoreDictionary)
                    {

                        var nodeGen = new Utilities.NodeNameGen();

                        client.Cypher
                            .Merge($"({nodeGen.NodeName}:Word {{ Lemma:'{diEl.Key}'}})")
                            .OnMatch()
                            .Set($"{nodeGen.NodeName}.PosScore = {diEl.Value.Positive.ToString(nfi)} ")
                            .Set($"{nodeGen.NodeName}.NegScore = {diEl.Value.Negative.ToString(nfi)} ")
                            .ExecuteWithoutResults();

                        nodeGen.Next();
                    }

                    client.Cypher
                        .Match($"(t:Tweet{{Id:'{tweet.Id}'}})")
                        .Match($"(w:Word {{ IsFirst: '{true}', TweetId : '{tweet.Id}' }})")
                        .Create("(t)-[r:BEGIN]->(w)")
                        .ExecuteWithoutResults();

                    var finalScore = ComputePatternScore(TweetId);
                    SaveScore(TweetId, finalScore.Item1, finalScore.Item2);
                    return finalScore;

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            };

            return new Tuple<double, double>(0, 0);
        }

        public Tuple<double, double> ComputePatternScore(String TweetId)
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();

                    //match (f:Word {TweetId:'961414872797011969'})-[r:NEXT]->(s:Word {TweetId:'961414872797011969',Type:'JJ'}) where f is null OR NOT(f.Type IN ['RBR','RB']) return f,s
                    var _startingAdjective = client.Cypher
                         .Match($"(w:Word{{TweetId:'{TweetId}', IsFirst:'True'}})")
                         .Where("w.Type in ['JJ','JJR','JJS']")
                         .ReturnDistinct(w => w.As<Word>()).Results as List<Word>;

                    List<Word> simpleAdjectives = client.Cypher
                        .Match($"(w1:Word{{TweetId:'{TweetId}'}})-[NEXT]->(w2:Word{{TweetId:'{TweetId}'}})")
                        .Where("w2.Type in ['JJ','JJR','JJS'] AND(w1 is null OR NOT(w1.Type IN ['RBR','RB']))")
                        .ReturnDistinct(w2 => w2.As<Word>()).Results as List<Word>;

                    simpleAdjectives = simpleAdjectives.Concat(_startingAdjective).ToList();

                    //match(s: Word { TweetId: '961409041686847493',Type: 'RB'})-[r: NEXT]->(t: Word { TweetId: '961409041686847493',Type: 'JJ'}) return s,t

                    var negatedAdjectives = client.Cypher
                        .Match($"(w1:Word{{TweetId:'{TweetId}',Type: 'RB'}})-[NEXT]->(w2:Word{{TweetId:'{TweetId}',Type:'JJ'}})")
                        .Where("toLower(w1.Lemma) CONTAINS 'not'")
                        .ReturnDistinct(w2 => w2.As<Word>()).Results;

                    //match(f: Word { TweetId: '961414872797011969',Type: 'RB'})-[r: NEXT]->(s: Word { TweetId: '961414872797011969',Type: 'RB'})-[rr: NEXT]->(t: Word { TweetId: '961414872797011969',Type: 'JJ'}) return f,s,t

                    var complexNegatedAdjectives = client.Cypher
                        .Match($"(w1:Word{{TweetId:'{TweetId}',Type: 'RB'}})-[n1:NEXT]->(w2:Word{{TweetId:'{TweetId}',Type: 'RB'}})-[n2:NEXT]->(w3:Word{{TweetId:'{TweetId}',Type:'JJ'}})")
                        .Where($"toLower(w2.Lemma) CONTAINS 'not'")
                        .ReturnDistinct(w3 => w3.As<Word>()).Results;

                    //match(s: Word { TweetId: '961388580647178240',Type: 'RBR'})-[r: NEXT]->(t: Word { TweetId: '961388580647178240',Type: 'JJ'}) return s,t

                    var alteratedAdjectivesPlus = client.Cypher
                        .Match($"(w1:Word{{TweetId:'{TweetId}',Type: 'RBR'}})-[NEXT]->(w2:Word{{TweetId:'{TweetId}',Type:'JJ'}})")
                        .Where($"toLower(w1.Lemma) IN ['very', 'more', 'most']")
                        .ReturnDistinct(w2 => w2.As<Word>()).Results;

                    var alteratedAdjectivesMinus = client.Cypher
                        .Match($"(w1:Word{{TweetId:'{TweetId}',Type: 'RBR'}})-[NEXT]->(w2:Word{{TweetId:'{TweetId}',Type:'JJ'}})")
                        .Where($"toLower(w1.Lemma) IN ['less', 'more', 'least']")
                        .ReturnDistinct(w2 => w2.As<Word>()).Results;

                    double scoreSimpleAdj = simpleAdjectives.Count() > 0 ? simpleAdjectives.ToList().Sum(x => x.PosScore.GetValueOrDefault(0)) - simpleAdjectives.ToList().Sum(x => x.NegScore.GetValueOrDefault(0)) : 0;
                    double percentSimpleAdj = simpleAdjectives.Count() > 0 ? simpleAdjectives.ToList().Sum(x => Math.Abs(x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0)) * 100) / simpleAdjectives.Count() : 0;

                    double scoreNegatedAdj = negatedAdjectives.Count() > 0 ? negatedAdjectives.ToList().Sum(x => x.NegScore.GetValueOrDefault(0)) - negatedAdjectives.ToList().Sum(x => x.PosScore.GetValueOrDefault(0)) : 0;
                    double percentNegateAdj = negatedAdjectives.Count() > 0 ? negatedAdjectives.ToList().Sum(x => Math.Abs(x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0)) * 100) / negatedAdjectives.Count() : 0;

                    double scoreComplexNegAdj = complexNegatedAdjectives.Count() > 0 ? complexNegatedAdjectives.ToList().Sum(x => x.NegScore.GetValueOrDefault(0)) - complexNegatedAdjectives.ToList().Sum(x => x.PosScore.GetValueOrDefault(0)) : 0;
                    double percenteComplexNegAdj = complexNegatedAdjectives.Count() > 0 ? complexNegatedAdjectives.ToList().Sum(x => Math.Abs(x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0)) * 100) / complexNegatedAdjectives.Count() : 0;

                    double scoreAlteratedAdjPlus = alteratedAdjectivesPlus.Count() > 0 ? alteratedAdjectivesPlus.ToList().Sum(x => weightPlus * (x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0))) : 0;
                    double percenteAlteratedAdjPlus = alteratedAdjectivesPlus.Count() > 0 ? alteratedAdjectivesPlus.ToList().Sum(x => Math.Abs(x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0)) * 100) / alteratedAdjectivesPlus.Count() : 0;

                    double scoreAlteratedAdjMinus = alteratedAdjectivesMinus.Count() > 0 ? alteratedAdjectivesMinus.ToList().Sum(x => weightMinus * (x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0))) : 0;
                    double percenteAlteratedAdjMinus = alteratedAdjectivesMinus.Count() > 0 ? alteratedAdjectivesMinus.ToList().Sum(x => Math.Abs(x.PosScore.GetValueOrDefault(0) - x.NegScore.GetValueOrDefault(0)) * 100) / alteratedAdjectivesMinus.Count() : 0;

                    var percentList = new List<double> { percentSimpleAdj, percentNegateAdj, percenteComplexNegAdj, percenteAlteratedAdjPlus, percenteAlteratedAdjMinus };
                    var finalPercent = (percentList.Any(x => x > 0)) ? percentList.Where(x => x > 0).Average() : 0;
                    finalPercent = Math.Round(finalPercent, 2);
                    var finalScore = scoreSimpleAdj + scoreNegatedAdj + scoreComplexNegAdj + scoreAlteratedAdjPlus + scoreAlteratedAdjMinus;

                    return new Tuple<double, double>(finalScore, finalPercent);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
                return new Tuple<double, double>(0, 0);
            }
        }

        public double? GetScore(String TweetId)
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {

                try
                {
                    client.Connect();

                    client.Cypher
                        .Match($"(t:Tweet{{Id:'{TweetId}'}})")
                        .Where("EXISTS(t.Score)")
                        .OnMatch()
                        .Return(t => t.As<Tweet>().Score);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
                return null;
            }
        }

        public void SaveScore(String TweetId, double score,  double percentage = 0)
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();

                    client.Cypher
                        .Match($"(t:Tweet{{Id:'{TweetId}'}} )")
                        .Set($"t.Score = {score.ToString().Replace(",", ".")}, t.Percentage = {String.Format("{0:0.00}", percentage).Replace(",",".")}")
                        .ExecuteWithoutResults();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }
        }

        protected Tweet GetTweet(String TweetId)
        {
            using (var client = new GraphClient(new Uri(@"http://localhost:7474/db/data"), "AdmUser", "pass"))
            {
                try
                {
                    client.Connect();
                    var tweet = client.Cypher
                    .Merge($"(t:Tweet{{Id:'{TweetId}'}})")
                    .Return(t => t.As<Tweet>())
                    .Results;

                    return tweet.FirstOrDefault();

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }

            return null;
        }

    }
}