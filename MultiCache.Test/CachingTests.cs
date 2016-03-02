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
                                              .Finish();
            multiCache.Get<string>(cacheKey);

            Assert.IsTrue(cache1.InternalCache.ContainsKey(cacheKey));
        }
    }
}