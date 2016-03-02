# Heading
Caching wrapper to facilitate multiple caching layers. 

Add multiple caching implementations which will be checked sequencial. If a caching value is found in one of the higher level caches it will be automatically saved to the lower level caches.

### Configuration
MutliCache caching levels work from top to bottom. In this example `InMemoryCache` will be called first. If the caching key does not exists `RedisCacheClient` will be checked.

    MultiCache multiCache = MultiCache.Configure()
                            .AddCacheLevel(new InMemoryCache()) // Level 0
                            .AddCacheLevel(new RedisCacheClient()) // Level 1
                            .Create()

If you have cache clients which might be slow to respond - due to for example network latency - you can group them together so they're reqeusted at the same time. The first client which responds with valid a result will be used.

    MultiCache multiCache = MultiCache.Configure()
                            .AddCacheLevel(new AzureCache(), new AmazonCache())
                            .Create()

### ServiceStack integration
MultiCache implements and uses the `ServiceStack.Interfaces.ICacheClient` interface. This way it can be used like any other caching implmentation in ServiceStack and existing ServiceStack caching implementations can be used by MultiCache.