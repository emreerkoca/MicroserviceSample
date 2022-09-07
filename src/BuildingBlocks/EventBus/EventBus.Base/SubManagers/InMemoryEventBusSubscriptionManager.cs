using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.Base.SubManagers
{
    internal class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public Func<string, string> eventNameGettter;

        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGettter)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();

            this.eventNameGettter = eventNameGettter;
        }

        public bool IsEmpty => !_handlers.Keys.Any();

        public void Clear() => _handlers.Clear();

        public void AddSubscription<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(THandler), eventName);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }

        private void AddSubscription(Type handlerType, string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            _handlers[eventName].Add(SubscriptionInfo.SetHandlerType(handlerType));
        }

        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;

            return eventNameGettter(eventName);
        }

        public Type GetEventTypeByName(string eventName)
        {
            return _eventTypes.SingleOrDefault(t => t.Name == eventName);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();

            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName)
        {
            return _handlers[eventName];
        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();

            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName)
        {
            return _handlers.ContainsKey(eventName);
        }

        public void RemoveSubscription<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, THandler>();
            var eventName = GetEventKey<T>();

            RemoveHandler(eventName, handlerToRemove);
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subscriptionInfo)
        {
            if (subscriptionInfo != null)
            {
                _handlers[eventName].Remove(subscriptionInfo);
            }

            if (!_handlers[eventName].Any())
            {
                _handlers.Remove(eventName);

                var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);

                if (eventType != null)
                {
                    _eventTypes.Remove(eventType);
                }

                RaiseOnEventRemoved(eventName);
            }
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;

            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, THandler>() where T : IntegrationEvent where THandler : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            return FindSubscriptionToRemove(eventName, typeof(THandler));
        }

        private SubscriptionInfo FindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType); 
        }
    }
}
