using System.Collections.Concurrent;

namespace AIChat.Shared.Plugins;

/// <summary>
/// 插件事件总线实现
/// </summary>
public class PluginEventBus : IPluginEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers;

    public PluginEventBus()
    {
        _handlers = new ConcurrentDictionary<Type, List<Func<object, Task>>>();
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        var wrappedHandler = new Func<object, Task>(async eventData =>
        {
            if (eventData is TEvent typedEvent)
            {
                await handler(typedEvent);
            }
        });

        _handlers.AddOrUpdate(eventType,
            new List<Func<object, Task>> { wrappedHandler },
            (key, existingHandlers) =>
            {
                existingHandlers.Add(wrappedHandler);
                return existingHandlers;
            });
    }

    public async Task PublishAsync<TEvent>(TEvent eventData) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            var tasks = handlers.Select(handler => handler(eventData));
            await Task.WhenAll(tasks);
        }
    }

    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IPluginEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            // 在实际应用中，这里需要更复杂的逻辑来匹配和移除特定的处理器
            // 现在简化处理，直接清空该类型的所有处理器
            handlers.Clear();
        }
    }
}