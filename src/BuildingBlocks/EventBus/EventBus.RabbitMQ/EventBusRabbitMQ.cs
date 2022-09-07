using EventBus.Base;
using EventBus.Base.Events;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;

namespace EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : BaseEventBus
    {
        RabbitMQPersistentConnection rabbitMQPersistentConnection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IModel _consumerChannel;

        public EventBusRabbitMQ(IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base(serviceProvider, eventBusConfig)
        {
            if (EventBusConfig.Connection != null)
            {
                if (EventBusConfig.Connection is ConnectionFactory)
                    _connectionFactory = EventBusConfig.Connection as ConnectionFactory;
                else
                {
                    var connJson = JsonConvert.SerializeObject(EventBusConfig.Connection, new JsonSerializerSettings()
                    {
                        // Self referencing loop detected for property 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    _connectionFactory = JsonConvert.DeserializeObject<ConnectionFactory>(connJson);
                }
            }
            else
                _connectionFactory = new ConnectionFactory(); //Create with default values

            rabbitMQPersistentConnection = new RabbitMQPersistentConnection(_connectionFactory, eventBusConfig.ConnectionRetryCount);

            _consumerChannel = CreateConsumerChannel();

            _eventBusSubscriptionManager.OnEventRemoved += _eventBusSubscriptionManager_OnEventRemoved;
        }

        private void _eventBusSubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            eventName = ProcessEventName(eventName);

            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            _consumerChannel.QueueUnbind(queue: eventName,
                exchange: EventBusConfig.DefaultTopicName,
                routingKey: eventName);


            if (_eventBusSubscriptionManager.IsEmpty)
            {
                _consumerChannel.Close();
            }
        }

        public override void Publish(IntegrationEvent @event)
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(EventBusConfig.ConnectionRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {

                });

            var eventName = @event.GetType().Name;
            eventName = ProcessEventName(eventName);

            _consumerChannel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            policy.Execute(() =>
            {
                var properties = _consumerChannel.CreateBasicProperties();
                properties.DeliveryMode = 2;

                _consumerChannel.BasicPublish(
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body);
            });
        }

        public override void Subscribe<T, THandler>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if(!_eventBusSubscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                if (!rabbitMQPersistentConnection.IsConnected)
                {
                    rabbitMQPersistentConnection.TryConnect();
                }

                _consumerChannel.QueueDeclare(queue: GetSubName(eventName),
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _consumerChannel.QueueBind(queue: GetSubName(eventName),
                    exchange: EventBusConfig.DefaultTopicName,
                    routingKey: eventName);
            }

            _eventBusSubscriptionManager.AddSubscription<T, THandler>();
            StartBasicConsume(eventName);
        }

        public override void UnSubscribe<T, THandler>()
        {
            _eventBusSubscriptionManager.RemoveSubscription<T, THandler>();
        }

        private IModel CreateConsumerChannel()
        {
            if (!rabbitMQPersistentConnection.IsConnected)
            {
                rabbitMQPersistentConnection.TryConnect();
            }

            var channel = rabbitMQPersistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: EventBusConfig.DefaultTopicName, type: "direct");

            return channel;
        }

        private void StartBasicConsume(string eventName)
        {
            if(_consumerChannel != null)
            {
                var consumer = new EventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: GetSubName(eventName),
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;

            eventName = ProcessEventName(eventName);
            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception)
            {
            }

            _consumerChannel.BasicAck(e.DeliveryTag, multiple: false);
        }
    }
}
