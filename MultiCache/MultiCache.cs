using System;
using System.Collections.Generic;
using ServiceStack.Caching;

namespace MultiCache
{
    /// <summary>
    /// Caching wrapper to facilitate multiple caching layers.
    /// 
    /// Add multiple caching implementations which will be checked sequencial. If a caching value is
    /// found in one of the higher level caches it will be automatically saved to the lower level caches.
    /// </summary>
    public sealed class MultiCache : ICacheClient
    {
        private MultiCacheConfiguration Configuration { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        internal MultiCache(MultiCacheConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Create new configuration to create MultiCache
        /// </summary>
        public static MultiCacheConfiguration Configure()
        {
            return new MultiCacheConfiguration();
        }

        /// <summary>
        /// Get dictionary of configured caching levels where key is level and value is caching level object
        /// </summary>
        public IDictionary<int, MultiCacheLevel> GetCacheLevels()
        {
            return Configuration.GetCacheLevels();
        }

        #region ServiceStack Interface

        /// <summary>
        /// ICacheClient - Dispose
        /// </summary>
        public void Dispose()
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.Dispose();
            }
        }

        /// <summary>
        /// ICacheClient - Remove
        /// </summary>
        public bool Remove(string key)
        {
            bool toReturn = false;

            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            { 
                if (cacheLevel.Remove(key))
                    toReturn = true;
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - RemoveAll
        /// </summary>
        public void RemoveAll(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.RemoveAll(keysList);
            }
        }

        /// <summary>
        /// ICacheClient - Get
        /// </summary>
        public T Get<T>(string key)
        {
            T toReturnValue = default(T);

            int firstCacheLevel = -1;
            int cacheLevel = -1;

            // Search for cache value
            foreach (KeyValuePair<int, MultiCacheLevel> entry in GetCacheLevels())
            {
                if (firstCacheLevel == -1)
                    firstCacheLevel = entry.Key;

                T value = entry.Value.Get<T>(key);
                if (value != null && !value.Equals(toReturnValue))
                {
                    // Found value, store it and current CacheClient level
                    toReturnValue = value;
                    cacheLevel = entry.Key;
                }
            }

            if (cacheLevel >= firstCacheLevel)
            {
                // Write cache value to higher cache layers
                foreach (KeyValuePair<int, MultiCacheLevel> entry in GetCacheLevels())
                {
                    if (entry.Key >= cacheLevel)
                    {
                        break;
                    }

                    // Set value
                    entry.Value.Set(key, toReturnValue);
                }
            }

            return toReturnValue;
        }

        /// <summary>
        /// ICacheClient - Increment
        /// </summary>
        public long Increment(string key, uint amount)
        {
            long newAmount = 0;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                newAmount = cacheLevel.Increment(key, amount);
            }

            return newAmount;
        }

        /// <summary>
        /// ICacheClient - Decrement
        /// </summary>
        public long Decrement(string key, uint amount)
        {
            long newAmount = 0;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                newAmount = cacheLevel.Decrement(key, amount);
            }

            return newAmount;
        }

        /// <summary>
        /// ICacheClient - Add
        /// </summary>
        public bool Add<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Add(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Set
        /// </summary>
        public bool Set<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Set(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Replace
        /// </summary>
        public bool Replace<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Replace(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Add with expiration
        /// </summary>
        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Add(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Set with expiration
        /// </summary>
        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Set(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Replace with expiration
        /// </summary>
        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Replace(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Add with expiration
        /// </summary>
        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Add(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Set with expiration
        /// </summary>
        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Set(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - Replace with expiration
        /// </summary>
        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                if (cacheLevel.Replace(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - FlushAll
        /// </summary>
        public void FlushAll()
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.FlushAll();
            }
        }

        /// <summary>
        /// ICacheClient - GetAll
        /// </summary>
        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);

            Dictionary<string, T> toReturn = new Dictionary<string, T>();
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                IDictionary<string, T> dict = cacheLevel.GetAll<T>(keysList);
                if (toReturn.Count == 0)
                {
                    toReturn = new Dictionary<string, T>(dict);
                }
                else
                {
                    foreach (KeyValuePair<string, T> entry in dict)
                    {
                        if (!toReturn.ContainsKey(entry.Key))
                        {
                            toReturn.Add(entry.Key, entry.Value);
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// ICacheClient - SetAll
        /// </summary>
        public void SetAll<T>(IDictionary<string, T> values)
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.SetAll(values);
            }
        }

        #endregion
    }
}