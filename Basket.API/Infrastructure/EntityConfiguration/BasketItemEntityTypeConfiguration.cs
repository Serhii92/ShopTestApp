using Basket.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.API.Infrastructure.EntityConfigurations
{
	class BasketItemEntityTypeConfiguration : IEntityTypeConfiguration<BasketItem>
	{
		public void Configure(EntityTypeBuilder<BasketItem> builder)
		{
			builder.ToTable("BasketItem");

			builder.HasKey(ci => ci.Id);

			builder.Property(ci => ci.Id)
				.ForSqlServerUseSequenceHiLo("catalog_brand_hilo")
				.IsRequired();
		}
	}
}
