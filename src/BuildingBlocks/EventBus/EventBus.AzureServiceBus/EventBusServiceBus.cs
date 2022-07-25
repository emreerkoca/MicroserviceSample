using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient _topicClient;
        private ManagementClient managementClient;
        private ILogger _logger;

        public EventBusServiceBus(IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base(serviceProvider, eventBusConfig)
        {
            managementClient = new ManagementClient(eventBusConfig.EventBusConnectionString);
            _topicClient = CreateTopicClient();
            _logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
        }
    
        private ITopicClient CreateTopicClient()
        {
            if (_topicClient == null || _topicClient.IsClosedOrClosing)
            {
                _topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
            }

            if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();

            return _topicClient;
        }

        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;

            eventName = ProcessEventName(eventName);

            var eventString = JsonConvert.SerializeObject(@event);
            var bodyArray = Encoding.UTF8.GetBytes(eventString);

            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArray,
                Label = eventName
            };

            _topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public override void Subscribe<T, THandler>()
        {
            var eventName = typeof(T).Name;

            eventName = ProcessEventName(eventName);

            if (!_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptionClient = CreateSubscriptionIfNotExists(eventName);

                RegisterSubscriptionClientMessageHandler(subscriptionClient);
            }

            _logger.LogInformation($"Subscribing to event {eventName} with {typeof(THandler).Name}");

            _eventBusSubscriptionManager.AddSubscription<T, THandler>();
        }

        public override void UnSubscribe<T, THandler>()
        {
            var eventName = typeof(T).Name;

            try
            {
                var subscriptionClient = CreateSubscriptionClient(eventName);

                subscriptionClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogWarning($"The messaging entity {eventName} could not be found.");
            }

            _logger.LogInformation($"Unsubscribing from event {eventName}");

            _eventBusSubscriptionManager.RemoveSubscription<T, THandler>();
        }

        private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    if (await ProcessEvent(ProcessEventName(eventName), messageData))
                    {
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 10,
                    AutoComplete = false
                });
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, $"Error handling message: {ex.Message} - Context: {context}");

            return Task.CompletedTask;
        }

        private ISubscriptionClient CreateSubscriptionIfNotExists(string eventName)
        {
            var subscriptionClient = CreateSubscriptionClient(eventName);
            var isExists = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
        
            if (!isExists)
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subscriptionClient);
            }

            CreateRuleIfNotExists(ProcessEventName(eventName), subscriptionClient);

            return subscriptionClient;
        }

        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {
            try
            {
                subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogWarning($"The messaging entity {RuleDescription.DefaultRuleName} could not found!");
            }
        }

        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool isRuleExists;

            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, eventName, eventName).GetAwaiter().GetResult();

                isRuleExists = rule != null;
            }
            catch (MessagingEntityNotFoundException)
            {
                isRuleExists = false;
            }

            if (!isRuleExists)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName},
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
        }

        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

        public override void Dispose()
        {
            base.Dispose();

            _topicClient.CloseAsync().GetAwaiter().GetResult();
            managementClient.CloseAsync().GetAwaiter().GetResult();
            _topicClient = null;
            managementClient = null;
        }
    }
}
    