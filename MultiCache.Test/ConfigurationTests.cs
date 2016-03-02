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
                                              .Finish();

            IDictionary<int, ICacheClient> cachingClients = multiCache.GetCacheClients();
            foreach (KeyValuePair<int, ICacheClient> client in cachingClients)
            {
                DummyCacheClient dummyClient = (DummyCacheClient)client.Value;

                Assert.AreEqual(client.Key, dummyClient.Index);
            }
        }

        [Test]
        public void CheckCacheClientOrderCustomLevels()
        {
            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(4, new DummyCacheClient(4))
                                              .AddCacheLevel(8, new DummyCacheClient(8))
                                              .AddCacheLevel(12, new DummyCacheClient(12))
                                              .Finish();

            IDictionary<int, ICacheClient> cachingClients = multiCache.GetCacheClients();
            foreach (KeyValuePair<int, ICacheClient> client in cachingClients)
            {
                DummyCacheClient dummyClient = (DummyCacheClient)client.Value;

                Assert.AreEqual(client.Key, dummyClient.Index);
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddCacheClientWithDuplicateLevel()
        {
            MultiCache.Configure()
                      .AddCacheLevel(new DummyCacheClient(0)) // 0
                      .AddCacheLevel(new DummyCacheClient(1)) // 1
                      .AddCacheLevel(1, new DummyCacheClient(2)); // 2, but insert as 1
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddCacheClientWithInvalidLevel()
        {
            MultiCache.Configure().AddCacheLevel(-1, new DummyCacheClient(1));
        }
    }
}
