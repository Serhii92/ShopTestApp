using System;
using RabbitMQ.Client;

namespace EventBusRabbitMQ
{
	public class DefaultRabbitMQPersistentConnection: IRabbitMQPersistentConnection
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public bool IsConnected { get; }
		public bool TryConnect()
		{
			throw new NotImplementedException();
		}

		public IModel CreateModel()
		{
			throw new NotImplementedException();
		}
	}
}
