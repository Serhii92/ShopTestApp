using Basket.API.Infrastructure.EntityConfigurations;
using Basket.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Product.API.Infrastructure.EntityConfigurations;

namespace Basket.API.Infrastructure
{
	public sealed class BasketContext : DbContext
	{
		public BasketContext(DbContextOptions<BasketContext> options) : base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<BasketItem> BasketItems { get; set; }
		public DbSet<BasketCheckout> BasketCheckouts { get; set; }
		public DbSet<CustomerBasket> CustomerBaskets { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new BasketItemEntityTypeConfiguration());
			builder.ApplyConfiguration(new BasketCheckoutEntityTypeConfiguration());
			builder.ApplyConfiguration(new CustomerBasketEntityTypeConfiguration());
		}

		public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<BasketContext>
		{
			public BasketContext CreateDbContext(string[] args)
			{
				var optionsBuilder = new DbContextOptionsBuilder<BasketContext>()
					.UseSqlServer("Server=.;Initial Catalog=eShopOnContainers.Services.BasketDb;Integrated Security=true");

				return new BasketContext(optionsBuilder.Options);
			}
		}
	}
}
