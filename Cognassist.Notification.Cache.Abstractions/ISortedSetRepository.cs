using System;
using System.Collections.Generic;

namespace Cognassist.Notification.Cache;

public interface ISortedSetRepository<T>
    where T : class
{
    void Add(string key, T value, double score);
    IEnumerable<(T Value, double Score)> GetRangeByRank(string key, int start = 0, int stop = -1, bool ascending = true);
    void Remove(string key, T value);
    long Count(string key);
    void Clear(string key);
}