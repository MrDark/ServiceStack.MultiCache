using System;
using System.Collections.Generic;
using ServiceStack.Caching;

namespace MultiCache.Test.Mocks
{
    public class DummyCacheClient : ICacheClient
    {
        public int Index { get; private set; }

        public readonly Dictionary<string, object> InternalCache = new Dictionary<string, object>();

        public DummyCacheClient(int index)
        {
            Index = index;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            return true;
        }

        public void RemoveAll(IEnumerable<string> keys)
        {
            
        }

        public T Get<T>(string key)
        {
            if (InternalCache.ContainsKey(key))
            {
                return (T)InternalCache[key];
            }

            return default(T);
        }

        public long Increment(string key, uint amount)
        {
            return 0;
        }

        public long Decrement(string key, uint amount)
        {
            return 0;
        }

        public bool Add<T>(string key, T value)
        {
            InternalCache.Add(key, value);

            return true;
        }

        public bool Set<T>(string key, T value)
        {
            InternalCache[key] = value;
            return true;
        }

        public bool Replace<T>(string key, T value)
        {
            return true;
        }

        public bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return true;
        }

        public bool Set<T>(string key, T value, DateTime expiresAt)
        {
            return true;
        }

        public bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return true;
        }

        public bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return true;
        }

        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            return true;
        }

        public bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return true;
        }

        public void FlushAll()
        {
            
        }

        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
        {
            return new Dictionary<string, T>();
        }

        public void SetAll<T>(IDictionary<string, T> values)
        {
            
        }
    }
}