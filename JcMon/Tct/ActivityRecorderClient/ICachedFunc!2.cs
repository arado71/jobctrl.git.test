namespace Tct.ActivityRecorderClient
{
    using System;
    using System.Runtime.InteropServices;

    public interface ICachedFunc<TKey, TValue>
    {
        void Clear();
        void ClearExpired();
        TValue GetOrCalculateValue(TKey key);
        void Remove(TKey key);
        bool TryGetValueFromCache(TKey key, out TValue value);
    }
}

