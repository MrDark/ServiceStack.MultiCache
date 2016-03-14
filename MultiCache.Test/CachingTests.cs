using MultiCache.Test.Mocks;
using NUnit.Framework;

namespace MultiCache.Test
{
    [TestFixture]
    public class CachingTests
    {
        [Test]
        public void RecacheValueToCacheHigherPriority()
        {
            string cacheKey = "MyCacheKey";

            DummyCacheClient cache1 = new DummyCacheClient(0);
            DummyCacheClient cache2 = new DummyCacheClient(1);
            cache2.Add(cacheKey, "Cached Value");

            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(cache1)
                                              .AddCacheLevel(cache2)
                                              .Create();
            multiCache.Get<string>(cacheKey);

            Assert.IsTrue(cache1.InternalCache.ContainsKey(cacheKey));
        }

        [Test]
        public void GetValueFromSlowCache()
        {
            MultiCache multiCache = MultiCache.Configure()
                                              .AddCacheLevel(0,
                                                             new SlowCacheClient<string>("Client1", 500),
                                                             new SlowCacheClient<string>("Client2", 300),
                                                             new SlowCacheClient<string>("Client3", 250))
                                              .Create();
            string output = multiCache.Get<string>("MyCacheKey");

            Assert.AreEqual("Client3", output);
        }
    }
}