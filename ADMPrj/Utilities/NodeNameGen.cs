using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ADMPrj.Utilities
{
    public class NodeNameGen
    {
        private static char[] charArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        private string _nodeName;
        public string NodeName
        {
            get
            {
                if (string.IsNullOrEmpty(_nodeName))
                    _nodeName = charArray.ToList().FirstOrDefault().ToString();

                return _nodeName;
            }
            private set { }
        }

        public string Next()
        {

            if (NodeName.ToCharArray().First() == charArray.Last())
                _nodeName = 'A' + NodeName;
            else
            {
                var i = charArray.Select((value, index) => new { value, index }).Where(x => x.value == NodeName.ToCharArray().FirstOrDefault())
                    .Select(x => x.index).Take(1);

                _nodeName = charArray[i.FirstOrDefault() + 1] + NodeName.Substring(1);
            }

            return NodeName;

        }
    }

    public static class Utils
    {
        public static string Purify(string twitterText)
        {
            Regex rex = new Regex(@"(@[A-Za-z0-9]+)|(\w+:\/\/\S+)");
            String text = twitterText.Clone().ToString();
            text = rex.Replace(text, String.Empty);

            text = new string(text.Where(c => !char.IsPunctuation(c) || c.ToString()=="'" || c.ToString() == "`" || c.ToString() == "’").ToArray());

            return text;
        }
    }
    
    public class Score
    {
        public double Positive { get; set; }
        public double Negative { get; set; }
    }
}