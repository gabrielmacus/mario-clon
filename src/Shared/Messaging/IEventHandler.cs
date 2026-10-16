namespace MarioClone.Shared.Messaging;

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    void Handle(TEvent @event);
}
