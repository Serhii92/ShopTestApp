using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Models
{
	public class CustomerBasket
	{
		public int BuyerId { get; set; }
		public List<BasketItem> Items { get; set; }

		//public CustomerBasket(int customerId)
		//{
		//	BuyerId = customerId;
		//	Items = new List<BasketItem>();
		//}
	}
}
