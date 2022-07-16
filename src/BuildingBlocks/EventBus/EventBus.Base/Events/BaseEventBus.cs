using EventBus.Base.Abstraction;
using EventBus.Base.SubManagers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.Base.Events
{
    public abstract class BaseEventBus : IEventBus
    {
        private readonly IServiceProvider _serviceProvider;
        public readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;

        public EventBusConfig EventBusConfig { get; set; }

        public BaseEventBus(IServiceProvider serviceProvider, EventBusConfig eventBusConfig)
        {
            _serviceProvider = serviceProvider;
            _eventBusSubscriptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
            EventBusConfig = eventBusConfig;
        }

        public virtual string ProcessEventName(string eventName)
        {
            if (EventBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimStart(EventBusConfig.EventNamePrefix.ToArray());
            }

            if (EventBusConfig.DeleteEventSuffix)
            {
                eventName = eventName.TrimEnd(EventBusConfig.EventNameSuffix.ToArray());
            }

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{EventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            EventBusConfig = null;
            _eventBusSubscriptionManager.Clear();
        }

        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);
            bool isProcessed = false;

            if (_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _eventBusSubscriptionManager.GetHandlersForEvent(eventName);

                using (var scope = _serviceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = _serviceProvider.GetService(subscription.HandlerType);

                        if (handler == null)
                            continue;

                        var eventType = _eventBusSubscriptionManager.GetEventTypeByName($"{EventBusConfig.EventNamePrefix}{eventName}{EventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }

                isProcessed = true;
            }

            return isProcessed;
        }

        public abstract void Publish(IntegrationEvent @event);

        public abstract void Subscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>;

        public abstract void UnSubscribe<T, THandler>()
            where T : IntegrationEvent
            where THandler : IIntegrationEventHandler<T>;
    }
}
