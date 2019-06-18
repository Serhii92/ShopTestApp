using System;
using Basket.API.Infrastructure;
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
	}
}