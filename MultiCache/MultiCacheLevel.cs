using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Caching;

namespace MultiCache
{
    public class MultiCacheLevel : ICacheClient
    {
        /// <summary>
        /// Collection of ICacheClients for this caching level
        /// </summary>
        private List<ICacheClient> CacheClients { get; } = new List<ICacheClient>();

        /// <summary>
        /// Caching Level
        /// </summary>
        internal int Level { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        internal MultiCacheLevel(int level)
        {
            Level = level;
        }

        /// <summary>
        /// Add cache client(s) to this caching level
        /// </summary>
        public void AddCacheClient(ICacheClient[] clients)
        {
            foreach (ICacheClient client in clients)
            {
                AddCacheClient(client);
            }
        }

        /// <summary>
        /// Get IEnumerable of configured cache clients
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICacheClient> GetCacheClients()
        {
            return CacheClients.ToArray();
        }

        /// <summary>
        /// Add cache client to this caching level
        /// </summary>
        private void AddCacheClient(ICacheClient cacheClient)
        {
            if (CacheClients.Contains(cacheClient))
            {
                throw new InvalidOperationException($"Caching level '{Level}' already contains a cache client '{cacheClient}'.");
            }

            CacheClients.Add(cacheClient);
        }

        /// <summary>
        /// Tries to get cached value from all configured caching clients. Result from the client which returns a valid result first is used.
        /// </summary>
        private async Task<T> GetAsyncAndWait<T>(string key)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            T defaultResult = default(T);

            // Create and run tasks for each client
            var tasks = CacheClients.Select(c => Task.Factory.StartNew(() =>
                                                                       {
                                                                           T value = c.Get<T>(key);
                                                                           return (cancellationToken.IsCancellationRequested) ? defaultResult : value;
                                                                       },
                                                                       cancellationToken,
                                                                       TaskCreationOptions.LongRunning,
                                                                       TaskScheduler.Default));

            HashSet<Task<T>> remainingTasks = new HashSet<Task<T>>(tasks);
            while (remainingTasks.Any())
            {
                var next = await Task.WhenAny(remainingTasks);
                if (!next.IsCanceled && next.Result != null && !next.Result.Equals(defaultResult))
                {
                    cancellationTokenSource.Cancel();
                    return next.Result;
                }

                remainingTasks.Remove(next);
            }

            return defaultResult;
        }

        #region ServiceStack ICacheClient Interface

        public void Dispose()
        {
            foreach (ICacheClient client in CacheClients)
            {
                client.Dispose();
            }
        }

        public bool Remove(string key)
        {
            bool toReturn = false;

            foreach (ICacheClient client in CacheClients)
            {
                if (client.Remove(key))
                    toReturn = true;
            }

            return toReturn;
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);

            foreach (ICacheClient client in CacheClients)
            {
                client.RemoveAll(keysList);
            }
        }

        public T Get<T>(string key)
        {
            T toReturnValue;

            if (CacheClients.Count == 1)
            {
                toReturnValue = CacheClients[0].Get<T>(key);
            }
            else
            {
                Task<T> task = GetAsyncAndWait<T>(key);
                toReturnValue = task.Result;
            }

            return toReturnValue;
        }

        public long Increment(string key, uint amount)
        {
            long newAmount = 0;
            
                foreach (ICacheClient client in CacheClients)
                {
                    newAmount = client.Increment(key, amount);
                }
            

            return newAmount;
        }

        public long Decrement(string key, uint amount)
        {
            long newAmount = 0;
                foreach (ICacheClient client in CacheClients)
                {
                    newAmount = client.Decrement(key, amount);
                }
            

            return newAmount;
        }

        public bool Add<T>(string key, T value)
        {
            bool toReturn = false;
            
                foreach (ICacheClient client in CacheClients)
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
            
                foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
            {
                client.FlushAll();
            }
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            List<string> keysList = new List<string>(keys);

            Dictionary<string, T> toReturn = new Dictionary<string, T>();
            foreach (ICacheClient client in CacheClients)
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
            foreach (ICacheClient client in CacheClients)
            {
                client.SetAll(values);
            }
        }

        #endregion
    }
}