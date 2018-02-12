using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tweet_test.Data
{
    public class DataManager
    {
        private static INeo4JDataHandler _neo4jInstance;
        public INeo4JDataHandler Neo4JInstance { get
            {
                if (_neo4jInstance == null)
                {
                    _neo4jInstance = new Neo4JDataHandler();
                }
                return _neo4jInstance;
            }
        }
    }
}