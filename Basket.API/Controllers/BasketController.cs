using System;
using System.Collections.Generic;
using Basket.API.Infrastructure;
using Basket.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BasketController : ControllerBase
	{
		private readonly BasketContext _basketContext;

		public BasketController(BasketContext basketContext)
		{
			_basketContext = basketContext ?? throw new ArgumentNullException(nameof(basketContext));
		}

		// GET api/basket
		[HttpGet]
		public ActionResult<IEnumerable<BasketItem>> Get()
		{
			//var priceChangedEvent = new ProductPriceChangedIntegrationEvent(1, 34, 55);
			//_eventBus.Publish(priceChangedEvent);
			//var result = _catalogContext.CatalogItems.ToList();

			return null;
		}
	}
}