namespace Rin.Editor;

static class DictionaryExtensions {
    public static TValue GetOrCreateDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TValue : new() where TKey : notnull {
        if (!dictionary.TryGetValue(key, out var value)) {
            value = new();
            dictionary[key] = value;
        }

        return value;
    }
}
