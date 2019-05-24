using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Product.API.Infrastructure.EntityConfigurations;
using Product.API.Models;

namespace Product.API.Infrastructure
{
	public sealed class CatalogContext : DbContext
	{
		public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
		{
			Database.EnsureCreated();
		}

		public DbSet<CatalogItem> CatalogItems { get; set; }
		public DbSet<CatalogBrand> CatalogBrands { get; set; }
		public DbSet<CatalogType> CatalogTypes { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new CatalogBrandEntityTypeConfiguration());
			builder.ApplyConfiguration(new CatalogTypeEntityTypeConfiguration());
			builder.ApplyConfiguration(new CatalogItemEntityTypeConfiguration());
		}

		public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
		{
			public CatalogContext CreateDbContext(string[] args)
			{
				var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
					.UseSqlServer("Server=.;Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb;Integrated Security=true");

				return new CatalogContext(optionsBuilder.Options);
			}
		}
	}
}
