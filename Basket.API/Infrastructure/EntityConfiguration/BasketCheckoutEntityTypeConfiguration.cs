using Basket.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Basket.API.Infrastructure.EntityConfigurations
{
	class BasketCheckoutEntityTypeConfiguration : IEntityTypeConfiguration<BasketCheckout>
	{
		public void Configure(EntityTypeBuilder<BasketCheckout> builder)
		{
			builder.ToTable("BasketCheckout");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
				.ForSqlServerUseSequenceHiLo("basket_checkout_hilo")
				.IsRequired();

		}
	}
}
