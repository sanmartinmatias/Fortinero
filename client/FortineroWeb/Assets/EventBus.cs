using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<string, Action> _events= new Dictionary<string, Action>();
    private static readonly Dictionary<string, Action<object>> _dataEvents= new Dictionary<string, Action<object>>();

    // Subscribe a method to an event
    public static Action Subscribe(string eventName, Action listener)
    {
        if (!_events.ContainsKey(eventName))
            _events[eventName] = null;
        
        _events[eventName] += listener;
        return listener;
    }

    public static void Subscribe(string name, Action<object> callback)
    {
        if (!_dataEvents.ContainsKey(name)) _dataEvents[name] = null;
        _dataEvents[name] += callback;
    }

    public static void Publish(string name, object data)
    {
        if (_dataEvents.TryGetValue(name, out var action))
            action(data);
    }

    // Unsubscribe to prevent memory leaks
    public static void Unsubscribe(string eventName, Action listener)
    {
        if (_events.ContainsKey(eventName))
            _events[eventName] -= listener;
    }

    // Fire the event
    public static void Publish(string eventName)
    {
        if (_events.ContainsKey(eventName))
            _events[eventName]?.Invoke();
    }
}

