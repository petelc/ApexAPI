
// using Apex.API.Infrastructure.Data;

// namespace Apex.API.IntegrationTests.Data;

// public abstract class BaseEfRepoTestFixture
// {
//   protected ApexDbContext _dbContext;

//   protected BaseEfRepoTestFixture()
//   {
//     var options = CreateNewContextOptions();
//     _dbContext = new ApexDbContext(options);
//   }

//   protected static DbContextOptions<ApexDbContext> CreateNewContextOptions()
//   {
//     var fakeEventDispatcher = Substitute.For<IDomainEventDispatcher>();
//     // Create a fresh service provider, and therefore a fresh
//     // InMemory database instance.
//     var serviceProvider = new ServiceCollection()
//         .AddEntityFrameworkInMemoryDatabase()
//         .AddScoped<IDomainEventDispatcher>(_ => fakeEventDispatcher)
//         .AddScoped<EventDispatchInterceptor>()
//         .BuildServiceProvider();

//     // Create a new options instance telling the context to use an
//     // InMemory database and the new service provider.
//     var interceptor = serviceProvider.GetRequiredService<EventDispatchInterceptor>();

//     var builder = new DbContextOptionsBuilder<AppDbContext>();
//     builder.UseInMemoryDatabase("cleanarchitecture")
//            .UseInternalServiceProvider(serviceProvider)
//            .AddInterceptors(interceptor);

//     return builder.Options;
//   }


// }
