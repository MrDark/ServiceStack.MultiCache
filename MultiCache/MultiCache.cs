using System;
using System.Collections.Generic;
using ServiceStack.Caching;

namespace MultiCache
{
    public class MultiCache : ICacheClient
    {
        private MultiCacheConfiguration Configuration { get; set; }

        internal MultiCache(MultiCacheConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static MultiCacheConfiguration Configure()
        {
            return new MultiCacheConfiguration();
        }

        public IDictionary<int, ICacheClient> GetCacheClients()
        {
            return Configuration.GetCacheClients();
        }

        #region ServiceStack Interface

        public void Dispose()
        {
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                client.Dispose();
            }
        }

        public bool Remove(string key)
        {
            bool toReturn = false;

            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Remove(key))
                    toReturn = true;
            }

            return toReturn;
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                client.RemoveAll(keysList);
            }
        }

        public T Get<T>(string key)
        {
            T toReturnValue = default(T);
            int cacheLevel = -1;

            // Search for cache value
            foreach (KeyValuePair<int, ICacheClient> entry in GetCacheClients())
            {
                T value = entry.Value.Get<T>(key);
                if (value != null && !value.Equals(toReturnValue))
                {
                    // Found value, store it and current CacheClient level
                    toReturnValue = value;
                    cacheLevel = entry.Key;
                }
            }

            if (cacheLevel >= 0)
            {
                // Write cache value to higher cache layers
                foreach (KeyValuePair<int, ICacheClient> entry in GetCacheClients())
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

        public long Increment(string key, uint amount)
        {
            long newAmount = 0;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                newAmount = client.Increment(key, amount);
            }

            return newAmount;
        }

        public long Decrement(string key, uint amount)
        {
            long newAmount = 0;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                newAmount = client.Decrement(key, amount);
            }

            return newAmount;
        }

        public bool Add<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Add(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Set<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Set(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Replace<T>(string key, T value)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Replace(key, value))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Add(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Set(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Replace(key, value, expiresAt))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Add(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Set(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            bool toReturn = false;
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                if (client.Replace(key, value, expiresIn))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        public void FlushAll()
        {
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                client.FlushAll();
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);

            Dictionary<string, T> toReturn = new Dictionary<string, T>();
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                IDictionary<string, T> dict = client.GetAll<T>(keysList);
                foreach (KeyValuePair<string, T> entry in dict)
                {
                    if (!toReturn.ContainsKey(entry.Key))
                    {
                        toReturn.Add(entry.Key, entry.Value);
                    }
                }
            }

            return toReturn;
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            foreach (ICacheClient client in GetCacheClients().Values)
            {
                client.SetAll(values);
            }
        }

        #endregion
    }
}