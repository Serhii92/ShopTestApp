using System;
using EventBus.Abstractions;
using EventBus.Events;

namespace EventBusRabbitMQ
{
	public class EventBusRabbitMQ: IEventBus
	{
		public void Publish(IntegrationEvent @event)
		{
			throw new NotImplementedException();
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
