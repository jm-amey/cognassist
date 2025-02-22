using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cognassist.Notification.Cache;

public class RedisSortedSetRepository<T>(IDatabase redisDb) : ISortedSetRepository<T>
where T : class
{
    public void Add(string key, T value, double score)
    {
        var serializedValue = RedisSortedSetRepository<T>.Serialise(value);
        redisDb.SortedSetAdd(key, serializedValue, score);
    }

    public IEnumerable<(T Value, double Score)> GetRangeByRank(string key, int start = 0, int stop = -1, bool ascending = true)
    {
        var order = ascending ? Order.Ascending : Order.Descending;
        var items = redisDb.SortedSetRangeByRankWithScores(key, start, stop, order);

        return items.Select(item => (Deserialise(item.Element), item.Score));
    }

    public void Remove(string key, T value)
    {
        var serializedValue = RedisSortedSetRepository<T>.Serialise(value);
        redisDb.SortedSetRemove(key, serializedValue);
    }

    public long Count(string key)
    {
        return redisDb.SortedSetLength(key);
    }

    public void Clear(string key)
    {
        redisDb.KeyDelete(key);
    }

    private static string Serialise(T value)
    {
        // You can use JSON serialization or any other method
        return JsonConvert.SerializeObject(value);
    }

    private static T Deserialise(RedisValue value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }
}