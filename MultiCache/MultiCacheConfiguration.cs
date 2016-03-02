using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;

namespace MultiCache
{
    public class MultiCacheConfiguration
    {
        private readonly SortedDictionary<int, ICacheClient> cachingClients = new SortedDictionary<int, ICacheClient>();

        /// <summary>
        /// Internal constructor so configuration can only be made via <see cref="MultiCache.Configure"/>
        /// </summary>
        internal MultiCacheConfiguration()
        {
        }

        /// <summary>
        /// Gets a dictionary of all configured cache clients and levels
        /// </summary>
        internal IDictionary<int, ICacheClient> GetCacheClients()
        {
            return cachingClients;
        }

        /// <summary>
        /// Add new cache client with priority one level higher than the previously added.
        /// </summary>
        public MultiCacheConfiguration AddCacheLevel(ICacheClient cacheClient)
        {
            int nextLevel = 0;
            if (cachingClients.Count > 0)
                nextLevel = cachingClients.Keys.Max() + 1;

            cachingClients.Add(nextLevel, cacheClient);

            return this;
        }

        /// <summary>
        /// Add new cache client with a set priority. The lower the level the higher the cache client its priority.
        /// </summary>
        /// <param name="level">Must be equal or greater than 0</param>
        /// <param name="cacheClient"></param>
        public MultiCacheConfiguration AddCacheLevel(int level, ICacheClient cacheClient)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Caching level has to be equal or greater than 0.");
            }
            if (cachingClients.ContainsKey(level))
            {
                throw new ArgumentException(nameof(level), $"CacheClient with level '{level}' already exists.");
            }

            cachingClients.Add(level, cacheClient);

            return this;
        }

        /// <summary>
        /// Create new MultiCache instance with current configuration.
        /// </summary>
        public MultiCache Finish()
        {
            return new MultiCache(this);
        }
    }
}
