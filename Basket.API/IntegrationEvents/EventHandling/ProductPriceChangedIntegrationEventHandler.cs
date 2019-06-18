using System;
using System.Linq;
using System.Threading.Tasks;
using Basket.API.Infrastructure;
using Basket.API.IntegrationEvents.Events;
using Basket.API.Models;
using EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Basket.API.IntegrationEvents.EventHandling
{
	public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
	{
		//private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;
		private readonly BasketContext _basketContext;

		public ProductPriceChangedIntegrationEventHandler(BasketContext basketContext)
		{
			//_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_basketContext = basketContext ?? throw new ArgumentNullException(nameof(basketContext));
		}

		public async Task Handle(ProductPriceChangedIntegrationEvent @event)
		{
			//using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
			//{
				//_logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

				//var userIds = _repository.GetUsers();

				//foreach (var id in userIds)
				//{
					var basket = await _basketContext.CustomerBaskets.FirstOrDefaultAsync();

					await UpdatePriceInBasketItems(@event.ProductId, @event.NewPrice, @event.OldPrice, basket);
				//}
			//}
		}

		private async Task UpdatePriceInBasketItems(int productId, decimal newPrice, decimal oldPrice, CustomerBasket basket)
		{
			string match = productId.ToString();
			var itemsToUpdate = basket?.Items?.Where(x => x.ProductId == match).ToList();

			if (itemsToUpdate != null)
			{
				//_logger.LogInformation("----- ProductPriceChangedIntegrationEventHandler - Updating items in basket for user: {BuyerId} ({@Items})", basket.BuyerId, itemsToUpdate);

				foreach (var item in itemsToUpdate)
				{
					if (item.UnitPrice == oldPrice)
					{
						var originalPrice = item.UnitPrice;
						item.UnitPrice = newPrice;
						item.OldUnitPrice = originalPrice;
					}
				}
				//await _repository.UpdateBasketAsync(basket);

				await _basketContext.SaveChangesAsync();
			}
		}
	}
}
