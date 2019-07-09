using System;
using EventBus;
using EventBus.Abstractions;
using EventBusRabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.API.Infrastructure;
using RabbitMQ.Client;

namespace Product.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string connection = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<CatalogContext>(options => options.UseSqlServer(connection));
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			services.AddIntegrationServices(Configuration);
			services.AddEventBus(Configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Catalog}/{action=Get}/{id?}");
			});
		}
	}

	public static class CustomExtensionMethods
	{
		public static IServiceCollection AddIntegrationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
			{
				var factory = new ConnectionFactory()
				{
					HostName = configuration["EventBusConnection"],
					DispatchConsumersAsync = true
				};

				if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
				{
					factory.UserName = configuration["EventBusUserName"];
				}

				if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
				{
					factory.Password = configuration["EventBusPassword"];
				}

				var retryCount = 5;
				if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
				{
					retryCount = int.Parse(configuration["EventBusRetryCount"]);
				}

				return new DefaultRabbitMQPersistentConnection(factory, retryCount);
			});

			return services;
		}

		public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
		{
			var subscriptionClientName = configuration["SubscriptionClientName"];
			services.AddSingleton<IEventBus, EventBusRabbitMQ.EventBusRabbitMQ>(sp =>
			{
				var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
				//var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
				//var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ.EventBusRabbitMQ>>();
				var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

				var retryCount = 5;
				if (!String.IsNullOrEmpty(configuration["EventBusRetryCount"]))
				{
					retryCount = Int32.Parse(configuration["EventBusRetryCount"]);
				}

				return new EventBusRabbitMQ.EventBusRabbitMQ(rabbitMQPersistentConnection, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
			});


			services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
			//services.AddTransient<OrderStatusChangedToAwaitingValidationIntegrationEventHandler>();
			//services.AddTransient<OrderStatusChangedToPaidIntegrationEventHandler>();

			return services;
		}
	}
}
