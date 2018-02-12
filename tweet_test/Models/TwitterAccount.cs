using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tweet_test.Models
{
    public class TwitterAccount
    {
        public ulong Id { get; set; }
        public String Name { get; set; }
        public String PublicName { get; set; }
        public Uri ImageUrl { get; set; }
    }
}