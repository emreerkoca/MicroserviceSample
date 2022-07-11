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

        private EventBusConfig eventBusConfig;

        public BaseEventBus(IServiceProvider serviceProvider, IEventBusSubscriptionManager eventBusSubscriptionManager, EventBusConfig eventBusConfig)
        {
            _serviceProvider = serviceProvider;
            _eventBusSubscriptionManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
            this.eventBusConfig = eventBusConfig;
        }

        public virtual string ProcessEventName(string eventName)
        {
            if (eventBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimStart(eventBusConfig.EventNamePrefix.ToArray());
            }

            if (eventBusConfig.DeleteEventSuffix)
            {
                eventName = eventName.TrimEnd(eventBusConfig.EventNameSuffix.ToArray());
            }

            return eventName;
        }

        public virtual string GetSubName(string eventName)
        {
            return $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }

        public virtual void Dispose()
        {
            eventBusConfig = null;
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

                        var eventType = _eventBusSubscriptionManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
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
