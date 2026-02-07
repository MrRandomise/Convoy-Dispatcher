using System;
using System.Collections.Generic;
using UnityEngine;
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            Debug.LogWarning($"Service {type.Name} already registered. Replacing.");
        }
        _services[type] = service;
    }

    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return service as T;
        }
        throw new InvalidOperationException($"Service {type.Name} not registered.");
    }

    public static void Clear() => _services.Clear();
}