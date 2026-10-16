using System;
using System.Collections.Generic;

namespace MarioClone.Shared.Messaging;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    public static void Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        Subscribe<TEvent>(handler.Handle);
    }

    public static void Subscribe<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        var type = typeof(TEvent);
        if (!_subscribers.ContainsKey(type))
        {
            _subscribers[type] = new List<Delegate>();
        }
        _subscribers[type].Add(callback);
    }

    public static void Unsubscribe<TEvent>(Action<TEvent> callback) where TEvent : IEvent
    {
        var type = typeof(TEvent);
        if (_subscribers.TryGetValue(type, out var list))
        {
            list.Remove(callback);
        }
    }

    public static void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (_subscribers.TryGetValue(typeof(TEvent), out var list))
        {
            foreach (var del in list)
            {
                ((Action<TEvent>)del)(@event);
            }
        }
    }

    public static void Clear()
    {
        _subscribers.Clear();
    }
}
