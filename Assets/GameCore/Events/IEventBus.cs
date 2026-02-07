using System;

public interface IEventBus
{
    void Subscribe<T>(Action<T> handler) where T : IGameEvent;
    void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    void Publish<T>(T gameEvent) where T : IGameEvent;
}

public interface IGameEvent { }