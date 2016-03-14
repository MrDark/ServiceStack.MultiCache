using System;
using System.Collections.Generic;
using MultiCache.Test.Mocks;
using NUnit.Framework;
using ServiceStack.Caching;

namespace MultiCache.Test
{
    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void CheckCacheClientOrder()
        {
            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(new DummyCacheClient(0))
                                              .AddCacheLevel(new DummyCacheClient(1))
                                              .AddCacheLevel(new DummyCacheClient(2))
                                              .Create();

            IDictionary<int, MultiCacheLevel> cachingClients = multiCache.GetCacheLevels();
            foreach (KeyValuePair<int, MultiCacheLevel> entry in cachingClients)
            {
                DummyCacheClient dummyClient = (DummyCacheClient)entry.Value.GetCacheClients()[0];

                Assert.AreEqual(entry.Key, dummyClient.Index);
            }
        }

        [Test]
        public void CheckCacheClientOrderCustomLevels()
        {
            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(4, new DummyCacheClient(4))
                                              .AddCacheLevel(8, new DummyCacheClient(8))
                                              .AddCacheLevel(12, new DummyCacheClient(12))
                                              .Create();

            IDictionary<int, MultiCacheLevel> cachingClients = multiCache.GetCacheLevels();
            foreach (KeyValuePair<int, MultiCacheLevel> entry in cachingClients)
            {
                DummyCacheClient dummyClient = (DummyCacheClient)entry.Value.GetCacheClients()[0];

                Assert.AreEqual(entry.Key, dummyClient.Index);
            }
        }

        [Test]        
        public void AddCacheClientWithDuplicateLevel()
        {
            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(new DummyCacheClient(0)) // 0
                                              .AddCacheLevel(new DummyCacheClient(1)) // 1
                                              .AddCacheLevel(1, new DummyCacheClient(2)) // 2, but insert as 1
                                              .Create();

            IList<ICacheClient> clients = multiCache.GetCacheLevels()[1].GetCacheClients();
            Assert.AreEqual(2, clients.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddCacheClientWithInvalidLevel()
        {
            MultiCache.Configure().AddCacheLevel(-1, new DummyCacheClient(1));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddDuplicateCacheClientForLevel()
        {
            DummyCacheClient cacheClient = new DummyCacheClient(0);

            MultiCache.Configure()
                      .AddCacheLevel(0,
                                     cacheClient,
                                     cacheClient)
                      .Create();

            MultiCache.Configure()
                      .AddCacheLevel(cacheClient)
                      .AddCacheLevel(0, cacheClient)
                      .Create();

        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddNullCacheClient()
        {
            MultiCache.Configure()
                      .AddCacheLevel()
                      .Create();

            MultiCache.Configure()
                      .AddCacheLevel(null)
                      .Create();

            MultiCache.Configure()
                      .AddCacheLevel(null, null)
                      .Create();
        }
    }
}
