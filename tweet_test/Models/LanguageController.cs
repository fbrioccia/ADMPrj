using edu.stanford.nlp.tagger.maxent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace tweet_test.Models
{
    public class LanguageController
    {
        public static string model = @"/wsj-0-18-bidirectional-nodistsim.tagger";
        public static char[] delimiterChars = { ' ', '.', ':', '\t' };

        public void ParseText(string text)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";


            // Loading POS Tagger
            var tagger = new MaxentTagger(HttpContext.Current.Server.MapPath("/bin") + model);

            // Text for tagging
            var tokenArray = text.ToLower().Split(delimiterChars);

            var wordScoreDictionary = new Dictionary<string, Score>();

            for (int i = 0; i < tokenArray.Length - 1; i++)
            {
                var taggedToken = tagger.tagString(tokenArray[i]);
                var nextToken = tagger.tagString(tokenArray[i + 1]);

                var firstTokenType = taggedToken.Replace(tokenArray[i] + "_", "").Trim();
                var secondTokenType = nextToken.Replace(tokenArray[i + 1] + "_", "").Trim();

                if (Constants.PartOfSpeech.IsAdjective(firstTokenType))
                {
                    if (!wordScoreDictionary.ContainsKey(tokenArray[i]))
                        wordScoreDictionary[tokenArray[i]] = new Score();//dataManager.GetWordScore(tokenArray[i]);
                }

                if (Constants.PartOfSpeech.IsAdjective(secondTokenType))
                {
                    if (!wordScoreDictionary.ContainsKey(tokenArray[i + 1]))
                        wordScoreDictionary[tokenArray[i + 1]] = new Score();//dataManager.GetWordScore(tokenArray[i + 1]);
                }

                var posScore = (wordScoreDictionary.ContainsKey(tokenArray[i])) ? wordScoreDictionary[tokenArray[i]].Positive : 0;
            }

        }
    }

    public class Score
    {
        public double Positive { get; set; }
        public double Negative { get; set; }
    }




}