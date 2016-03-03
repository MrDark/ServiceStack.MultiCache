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
        /// Create new configuration for setting up MultiCache
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
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key">The identifier for the item to delete.</param>
        /// <returns>
        /// true if the item was successfully removed from the cache; false otherwise.
        /// </returns>
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
        /// Removes the cache for all the keys provided.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public void RemoveAll(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.RemoveAll(keysList);
            }
        }

        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The identifier for the item to retrieve.</param>
        /// <returns>
        /// The retrieved item, or <value>null</value> if the key was not found.
        /// </returns>
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
        /// Increments the value of the specified key by the given amount. 
        /// The operation is atomic and happens on the server.
        /// A non existent value at key starts at 0
        /// </summary>
        /// <param name="key">The identifier for the item to increment.</param>
        /// <param name="amount">The amount by which the client wants to increase the item.</param>
        /// <returns>
        /// The new value of the item or -1 if not found.
        /// </returns>
        /// <remarks>The item must be inserted into the cache before it can be changed. The item must be inserted as a <see cref="T:System.String"/>. The operation only works with <see cref="System.UInt32"/> values, so -1 always indicates that the item was not found.</remarks>
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
        /// Increments the value of the specified key by the given amount. 
        /// The operation is atomic and happens on the server.
        /// A non existent value at key starts at 0
        /// </summary>
        /// <param name="key">The identifier for the item to increment.</param>
        /// <param name="amount">The amount by which the client wants to decrease the item.</param>
        /// <returns>
        /// The new value of the item or -1 if not found.
        /// </returns>
        /// <remarks>The item must be inserted into the cache before it can be changed. The item must be inserted as a <see cref="T:System.String"/>. The operation only works with <see cref="System.UInt32"/> values, so -1 always indicates that the item was not found.</remarks>
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
        /// Adds a new item into the cache at the specified cache key only if the cache is empty.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
        /// <remarks>The item does not expire unless it is removed due memory pressure.</remarks>
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
        /// Sets an item into the cache at the cache key specified regardless if it already exists or not.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
        /// <remarks>The item does not expire unless it is removed due memory pressure.</remarks>
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
        /// Replaces the item at the cachekey specified only if an items exists at the location already.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
        /// <remarks>The item does not expire unless it is removed due memory pressure.</remarks>
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
        /// Add the value with key to the cache, set to expire at specified DateTime.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresAt">The date/time when the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Add or replace the value with key to the cache, set to expire at specified DateTime.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresAt">The date/time when the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Replace the value with key in the cache, set to expire at specified DateTime.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresAt">The date/time when the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Add the value with key to the cache, set to expire after specified TimeSpan.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresIn">The timespan after which the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Add or replace the value with key to the cache, set to expire after specified TimeSpan.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresIn">The timespan after which the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Replace the value with key in the cache, set to expire after specified TimeSpan.
        /// </summary>
        /// <param name="key">The key used to reference the item.</param>
        /// <param name="value">The object to be inserted into the cache.</param>
        /// <param name="expiresIn">The timespan after which the cache should invalidate.</param>
        /// <returns>
        /// true if the item was successfully stored in the cache; false otherwise.
        /// </returns>
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
        /// Invalidates all data on the cache.
        /// </summary>
        public void FlushAll()
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.FlushAll();
            }
        }

        /// <summary>
        /// Retrieves multiple items from the cache. 
        /// The default value of T is set for all keys that do not exist.
        /// </summary>
        /// <param name="keys">The list of identifiers for the items to retrieve.</param>
        /// <returns>
        /// a Dictionary holding all items indexed by their key.
        /// </returns>
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
        /// Sets multiple items to the cache. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">The values.</param>
        public void SetAll<T>(IDictionary<string, T> values)
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.SetAll(values);
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Call dispose on all cache clients
        /// </summary>
        public void Dispose()
        {
            foreach (MultiCacheLevel cacheLevel in GetCacheLevels().Values)
            {
                cacheLevel.Dispose();
            }
        }

        #endregion
    }
}