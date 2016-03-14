using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;

namespace MultiCache
{
    public sealed class MultiCacheConfiguration
    {
        private readonly SortedDictionary<int, MultiCacheLevel> cachingLevels = new SortedDictionary<int, MultiCacheLevel>();

        /// <summary>
        /// Internal constructor so configuration can only be made via <see cref="MultiCache.Configure"/>
        /// </summary>
        internal MultiCacheConfiguration()
        {
        }

        /// <summary>
        /// Gets a dictionary of all configured caching levels
        /// </summary>
        internal IDictionary<int, MultiCacheLevel> GetCacheLevels()
        {
            return cachingLevels;
        }

        /// <summary>
        /// Add new cache client with a set priority. The lower the level the higher the cache client its priority.
        /// </summary>
        /// <param name="level">Must be equal or greater than 0</param>
        /// <param name="cacheClients">One or more caching implentations to add for this level</param>
        /// <returns></returns>
        public MultiCacheConfiguration AddCacheLevel(int level, params ICacheClient[] cacheClients)
        {
            if (cacheClients == null || cacheClients.Length == 0)
            {
                throw new ArgumentNullException(nameof(cacheClients));
            }
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level), "Caching level has to be equal or greater than 0.");
            }

            MultiCacheLevel cacheLevel;
            if (!cachingLevels.TryGetValue(level, out cacheLevel))
            {
                cacheLevel = new MultiCacheLevel(level);
                cachingLevels.Add(cacheLevel.Level, cacheLevel);
            }

            cacheLevel.AddCacheClient(cacheClients);

            return this;
        }

        /// <summary>
        /// Add new cache client with priority one level higher than the previously added.
        /// </summary>
        public MultiCacheConfiguration AddCacheLevel(params ICacheClient[] cacheClients)
        {
            int nextLevel = 0;
            if (cachingLevels.Count > 0)
            {
                nextLevel = cachingLevels.Keys.Max() + 1;
            }

            AddCacheLevel(nextLevel, cacheClients);

            return this;
        }

        /// <summary>
        /// Create new MultiCache instance with current configuration.
        /// </summary>
        public MultiCache Create()
        {
            return new MultiCache(this);
        }
    }
}
