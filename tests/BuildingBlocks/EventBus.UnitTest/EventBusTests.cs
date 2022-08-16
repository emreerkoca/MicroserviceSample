using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events.EventHandlers;
using EventBus.UnitTest.Events.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBus.UnitTest
{
    public class EventBusTests
    {
        private ServiceCollection services;

        public EventBusTests()
        {
            services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
        }

        [Fact]
        public void subscribe_event_on_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(serviceProvider =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), serviceProvider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
        }

        [Fact]
        public void subscribe_event_on_azure_test()
        {
            services.AddSingleton<IEventBus>(serviceProvider =>
            {
                return EventBusFactory.Create(GetAzureConfig(), serviceProvider);
            });


            var serviceProvider = services.BuildServiceProvider();
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            //eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            Task.Delay(2000).Wait();
        }

        [Fact]
        public void send_message_to_rabbitmq_test()
        {
            services.AddSingleton<IEventBus>(serviceProvider =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), serviceProvider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }

        [Fact]
        public void send_message_to_azure_test()
        {
            services.AddSingleton<IEventBus>(serviceProvider =>
            {
                return EventBusFactory.Create(GetAzureConfig(), serviceProvider);
            });

            var serviceProvider = services.BuildServiceProvider();
            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }

        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "MictoserviceSampleEventBus",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",
                //Connection = new ConnectionFactory()
                //{ 
                //    HostName = "localhost",
                //    Port = 15672,
                //    UserName = "guest",
                //    Password = "guest"
                //}
            };
        }

        private EventBusConfig GetAzureConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 5,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "MictoserviceSampleEventBus",
                EventBusType = EventBusType.AzureServiceBus,
                EventNameSuffix = "IntegrationEvent",
                EventBusConnectionString = ""
            };
        }
    }
}