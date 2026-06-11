using System;
using System.Collections.Generic;

/// <summary>
/// 제네릭 타입 안전 EventBus.
/// Subscribe/Unsubscribe/Publish 모두 string 이벤트 키 + 타입 파라미터로 사용.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<string, Delegate> _listeners = new();

    public static void Subscribe<T>(string eventName, Action<T> callback)
    {
        if (!_listeners.ContainsKey(eventName))
            _listeners[eventName] = null;
        _listeners[eventName] = (Action<T>)_listeners[eventName] + callback;
    }

    public static void Unsubscribe<T>(string eventName, Action<T> callback)
    {
        if (_listeners.ContainsKey(eventName))
            _listeners[eventName] = (Action<T>)_listeners[eventName] - callback;
    }

    public static void Publish<T>(string eventName, T data)
    {
        if (_listeners.TryGetValue(eventName, out var del))
            ((Action<T>)del)?.Invoke(data);
    }
}
