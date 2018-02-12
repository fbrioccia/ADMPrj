using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crafted.Twitter;
using LinqToTwitter;
using tweet_test.Models;
namespace tweet_test.TwitterLogic
{
    public class TwitterHandler
    {

        private SingleUserAuthorizer userAu;
        private const string ConsumerKey = "F3VPz5Z9568MqteehzgZL2Tic";
        private const string ConsumerSecret = "qhJxGppJ8XDwU7JHHod5hTchPz2POE489VDlBzaqlxIczHgwrp";
        private const string AccessToken = "8PJ44rWIF3NvzV24gwKumC4JGiQHFmIVSrJyVrXdmn0m6";
        private const string OAuthToken = "919662004331319298-Tjst0AF995WO8IcoNxpb0N6HxnBVUlu";
        public TwitterHandler()
        {

            userAu = new SingleUserAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = TwitterHandler.ConsumerKey,
                    ConsumerSecret = TwitterHandler.ConsumerSecret,
                    OAuthToken = TwitterHandler.OAuthToken,
                    OAuthTokenSecret = TwitterHandler.AccessToken
                }
            };
        }

        public List<SeekResults> TSearch(string arguments)
        {
            var result = new List<SeekResults>();
            if(String.IsNullOrEmpty(arguments))
                return result;

            var argArray = arguments.Split(' ');

            if (argArray.All((x=>String.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x))))
                return result;

            var context = new TwitterContext(userAu);
            var searchResponse = (from search in context.Search
                                  where search.Type == SearchType.Search && search.SearchLanguage == "en" &&
                                        search.Query == String.Join(" ", argArray) && search.Count == 500 &&
                                         search.ResultType == ResultType.Recent &&
                                        search.TweetMode == TweetMode.Extended 
                                  select search)
                .Single();

            searchResponse.Statuses
                .Where(x => x.RetweetedStatus.StatusID == 0).ToList()
            .ForEach(entry => result.Add(new SeekResults
            {
                account = new TwitterAccount() {
                    Id = ulong.Parse(entry.User.UserIDResponse),
                    ImageUrl = new Uri(entry.User.ProfileImageUrl),
                    Name = entry.User.Name, PublicName = entry.User.Name },
                tweet = new Tweet() { Id = entry.StatusID.ToString() , Text = entry.FullText }
            }));

            return result.Take(10).ToArray().ToList();
        }

    }
}