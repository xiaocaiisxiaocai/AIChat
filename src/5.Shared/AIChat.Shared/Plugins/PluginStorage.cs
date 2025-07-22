using System.Collections.Concurrent;
using System.Text.Json;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件存储实现 - 基于内存的简单存储
/// </summary>
public class PluginStorage : IPluginStorage
{
    private readonly ConcurrentDictionary<string, object> _storage;

    public PluginStorage()
    {
        _storage = new ConcurrentDictionary<string, object>();
    }

    public async Task<T?> GetAsync<T>(string pluginId, string key)
    {
        await Task.CompletedTask;

        var storageKey = GetStorageKey(pluginId, key);
        if (_storage.TryGetValue(storageKey, out var value))
        {
            if (value is T directValue)
                return directValue;

            if (value is string jsonValue && typeof(T) != typeof(string))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(jsonValue);
                }
                catch
                {
                    return default(T);
                }
            }

            // 尝试转换类型
            try
            {
                return (T)value;
            }
            catch
            {
                return default(T);
            }
        }

        return default(T);
    }

    public async Task SetAsync(string pluginId, string key, object value)
    {
        await Task.CompletedTask;

        var storageKey = GetStorageKey(pluginId, key);
        
        if (value is string stringValue)
        {
            _storage.AddOrUpdate(storageKey, stringValue, (k, v) => stringValue);
        }
        else
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value);
                _storage.AddOrUpdate(storageKey, jsonValue, (k, v) => jsonValue);
            }
            catch
            {
                // 如果序列化失败，直接存储对象引用
                _storage.AddOrUpdate(storageKey, value, (k, v) => value);
            }
        }
    }

    public async Task RemoveAsync(string pluginId, string key)
    {
        await Task.CompletedTask;

        var storageKey = GetStorageKey(pluginId, key);
        _storage.TryRemove(storageKey, out _);
    }

    public async Task<bool> ExistsAsync(string pluginId, string key)
    {
        await Task.CompletedTask;

        var storageKey = GetStorageKey(pluginId, key);
        return _storage.ContainsKey(storageKey);
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pluginId)
    {
        await Task.CompletedTask;

        var prefix = $"{pluginId}:";
        var keys = _storage.Keys
            .Where(key => key.StartsWith(prefix))
            .Select(key => key.Substring(prefix.Length))
            .ToList();

        return keys;
    }

    public async Task ClearAsync(string pluginId)
    {
        await Task.CompletedTask;

        var prefix = $"{pluginId}:";
        var keysToRemove = _storage.Keys
            .Where(key => key.StartsWith(prefix))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _storage.TryRemove(key, out _);
        }
    }

    private static string GetStorageKey(string pluginId, string key)
    {
        return $"{pluginId}:{key}";
    }
}