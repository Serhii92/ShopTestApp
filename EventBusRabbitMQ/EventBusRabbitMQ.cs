using System;
using System.Text;
using EventBus.Abstractions;
using EventBus.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace EventBusRabbitMQ
{
	public class EventBusRabbitMQ: IEventBus
	{
		public void Publish(IntegrationEvent @event)
		{
			//{
			//	var eventName = @event.GetType().Name;
			//	var factory = new ConnectionFactory() { HostName = _connectionString };
			//	using (var connection = factory.CreateConnection())
			//	using (var channel = connection.CreateModel())
			//	{
			//		channel.ExchangeDeclare(exchange: _brokerName,
			//			type: "direct");
			//		string message = JsonConvert.SerializeObject(@event);
			//		var body = Encoding.UTF8.GetBytes(message);
			//		channel.BasicPublish(exchange: _brokerName,
			//			routingKey: eventName,
			//			basicProperties: null,
			//			body: body);
			//	}
			//}
		}

		public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
		{
			throw new NotImplementedException();
		}

		public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
		{
			throw new NotImplementedException();
		}

		public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
		{
			throw new NotImplementedException();
		}
	}
}
