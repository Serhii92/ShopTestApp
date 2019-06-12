using Basket.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.API.Infrastructure.EntityConfigurations
{
	class CustomerBasketEntityTypeConfiguration : IEntityTypeConfiguration<CustomerBasket>
	{
		public void Configure(EntityTypeBuilder<CustomerBasket> builder)
		{
			builder.ToTable("CustomerBasket");

			builder.Property(ci => ci.BuyerId)
				.ForSqlServerUseSequenceHiLo("customer_busket_hilo")
				.IsRequired();

		}
	}
}
