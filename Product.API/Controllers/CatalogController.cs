using System;
using System.Collections.Generic;
using System.Linq;
using EventBus.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Product.API.Infrastructure;
using Product.API.IntegrationEvents.Events;
using Product.API.Models;

namespace Product.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CatalogController : ControllerBase
	{
		private readonly CatalogContext _catalogContext;
		private readonly IEventBus _eventBus;

		public CatalogController(CatalogContext context, IEventBus eventBus)
		{
			_catalogContext = context ?? throw new ArgumentNullException(nameof(context));

			_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
		}

		// GET api/catalog
		[HttpGet]
		public ActionResult<IEnumerable<CatalogItem>> Get()
		{
			var priceChangedEvent = new ProductPriceChangedIntegrationEvent(1, 34, 55);
			_eventBus.Publish(priceChangedEvent);
			var result = _catalogContext.CatalogItems.ToList();
			return result;
		}

	}
}
