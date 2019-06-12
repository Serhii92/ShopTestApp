using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Product.API.Infrastructure;
using Product.API.Models;

namespace Product.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CatalogController : ControllerBase
	{
		private readonly CatalogContext _catalogContext;

		public CatalogController(CatalogContext context)
		{
			_catalogContext = context ?? throw new ArgumentNullException(nameof(context));
		}

		// GET api/catalog
		[HttpGet]
		public ActionResult<IEnumerable<CatalogItem>> Get()
		{
			var result = _catalogContext.CatalogItems.ToList();
			return result;
		}

	}
}
