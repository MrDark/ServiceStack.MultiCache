# Heading
Caching wrapper to facilitate multiple caching layers. 

Add multiple caching implementations which will be checked sequencial. If a caching value is found in one of the higher level caches it will be automatically saved to the lower level caches.

### Configuration
    MultiCache multiCache = MultiCache.Configure()
                            .AddCacheLevel(new InMemoryCache()) // Level 0
                            .AddCacheLevel(new RedisCacheClient()) // Level 1
                            .Finish()
MutliCache caching levels work from top to bottom. In this example `InMemoryCache` will be called first. If the caching key does not exists `RedisCacheClient` will be checked.

### ServiceStack integration
MultiCache implements and uses the `ServiceStack.Interfaces.ICacheClient` interface. This way it can be used like any other caching implmentation in ServiceStack and existing ServiceStack caching implementations can be used by MultiCache.

### To-Do
* Allow multiple caches to have the same caching level. Both will be called at the same time, the first one returning a result will be used.