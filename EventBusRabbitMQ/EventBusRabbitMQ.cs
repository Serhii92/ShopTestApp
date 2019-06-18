﻿using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EventBus;
using EventBus.Abstractions;
using EventBus.Events;
using EventBus.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBusRabbitMQ
{
	public class EventBusRabbitMQ : IEventBus
	{
		const string BROKER_NAME = "eshop_event_bus";

		private readonly IRabbitMQPersistentConnection _persistentConnection;
		//private readonly ILogger<EventBusRabbitMQ> _logger;
		private readonly IEventBusSubscriptionsManager _subsManager;
		//private readonly ILifetimeScope _autofac;
		//private readonly string AUTOFAC_SCOPE_NAME = "eshop_event_bus";
		private readonly int _retryCount;

		private IModel _consumerChannel;
		private string _queueName;

		public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection,
			//ILogger<EventBusRabbitMQ> logger,
			//ILifetimeScope autofac, 
			IEventBusSubscriptionsManager subsManager,
			string queueName = null,
			int retryCount = 5)
		{
			_persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
			//_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
			_queueName = queueName;
			_consumerChannel = CreateConsumerChannel();
			//_autofac = autofac;
			_retryCount = retryCount;
			_subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
		}

		private void SubsManager_OnEventRemoved(object sender, string eventName)
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			using (var channel = _persistentConnection.CreateModel())
			{
				channel.QueueUnbind(queue: _queueName,
					exchange: BROKER_NAME,
					routingKey: eventName);

				if (_subsManager.IsEmpty)
				{
					_queueName = string.Empty;
					_consumerChannel.Close();
				}
			}
		}

		public void Publish(IntegrationEvent @event)
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			var policy = RetryPolicy.Handle<BrokerUnreachableException>()
				.Or<SocketException>()
				.WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
				{
					//_logger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id, $"{time.TotalSeconds:n1}", ex.Message);
				});

			using (var channel = _persistentConnection.CreateModel())
			{
				var eventName = @event.GetType()
					.Name;

				channel.ExchangeDeclare(exchange: BROKER_NAME,
					type: "direct");

				var message = JsonConvert.SerializeObject(@event);
				var body = Encoding.UTF8.GetBytes(message);

				policy.Execute(() =>
				{
					var properties = channel.CreateBasicProperties();
					properties.DeliveryMode = 2; // persistent

					channel.BasicPublish(exchange: BROKER_NAME,
						routingKey: eventName,
						mandatory: true,
						basicProperties: properties,
						body: body);
				});
			}
		}

		public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
		{
			var eventName = _subsManager.GetEventKey<T>();
			DoInternalSubscription(eventName);

			//_logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());

			_subsManager.AddSubscription<T, TH>();
			StartBasicConsume();
		}

		private void DoInternalSubscription(string eventName)
		{
			var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
			if (!containsKey)
			{
				if (!_persistentConnection.IsConnected)
				{
					_persistentConnection.TryConnect();
				}

				using (var channel = _persistentConnection.CreateModel())
				{
					channel.QueueBind(queue: _queueName,
						exchange: BROKER_NAME,
						routingKey: eventName);
				}
			}
		}

		public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
		{
			var eventName = _subsManager.GetEventKey<T>();

			//_logger.LogInformation("Unsubscribing from event {EventName}", eventName);

			_subsManager.RemoveSubscription<T, TH>();
		}

		public void Dispose()
		{
			if (_consumerChannel != null)
			{
				_consumerChannel.Dispose();
			}

			_subsManager.Clear();
		}

		private void StartBasicConsume()
		{
			if (_consumerChannel != null)
			{
				var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

				consumer.Received += Consumer_Received;

				_consumerChannel.BasicConsume(
					queue: _queueName,
					autoAck: false,
					consumer: consumer);
			}
			else
			{
				//_logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
			}
		}

		private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
		{
			var eventName = eventArgs.RoutingKey;
			var message = Encoding.UTF8.GetString(eventArgs.Body);

			try
			{
				if (message.ToLowerInvariant().Contains("throw-fake-exception"))
				{
					throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
				}

				await ProcessEvent(eventName, message);
			}
			catch (Exception ex)
			{
				//_logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
			}

			// Even on exception we take the message off the queue.
			// in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
			// For more information see: https://www.rabbitmq.com/dlx.html
			_consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
		}

		private IModel CreateConsumerChannel()
		{
			if (!_persistentConnection.IsConnected)
			{
				_persistentConnection.TryConnect();
			}

			var channel = _persistentConnection.CreateModel();

			channel.ExchangeDeclare(exchange: BROKER_NAME,
									type: "direct");

			channel.QueueDeclare(queue: _queueName,
								 durable: true,
								 exclusive: false,
								 autoDelete: false,
								 arguments: null);

			channel.CallbackException += (sender, ea) =>
			{
				_consumerChannel.Dispose();
				_consumerChannel = CreateConsumerChannel();
				StartBasicConsume();
			};

			return channel;
		}

		private async Task ProcessEvent(string eventName, string message)
		{
			if (_subsManager.HasSubscriptionsForEvent(eventName))
			{
				//TODO: Investigate Autofac and lifetime scope, fix flow
				//using (var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
				//{
				var subscriptions = _subsManager.GetHandlersForEvent(eventName);
				foreach (var subscription in subscriptions)
				{
					//if (subscription.IsDynamic)
					//{
					//	var handler = scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
					//	if (handler == null) continue;
					//	dynamic eventData = JObject.Parse(message);
					//	await handler.Handle(eventData);
					//}
					//else
					//{
					//	var handler = scope.ResolveOptional(subscription.HandlerType);
					//	if (handler == null) continue;
					//	var eventType = _subsManager.GetEventTypeByName(eventName);
					//	var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
					//	var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
					//	await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
					//}

					throw new NotImplementedException();
				}
				//}
			}
		}
	}
}
