using System;
using System.Linq;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Z.EntityFramework.Plus;

namespace TestEfCoreCachable
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<TestDbContext>(
                options => options.UseSqlServer("Data Source=.;Initial Catalog=TestDb;Integrated security=True;MultipleActiveResultSets=True", sqlOptions => { }),
                ServiceLifetime.Transient);

            //Setup second level cachine for EF
            serviceCollection.AddEFSecondLevelCache();
            serviceCollection.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            serviceCollection.AddSingleton(typeof(ICacheManagerConfiguration),
                new ConfigurationBuilder()
                    .WithJsonSerializer()
                    .WithMicrosoftMemoryCacheHandle()
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                    .Build());

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var dbContext = serviceProvider.GetService<TestDbContext>();
            dbContext.Database.Migrate();

            dbContext.Users.Delete();
            dbContext.Users.Add(new User { Id = "test1" });
            dbContext.Users.Add(new User { Id = "test2" });
            dbContext.Users.Add(new User { Id = "test3" });
            dbContext.SaveChanges();


            EFServiceProvider.ApplicationServices = serviceProvider;

            var single1 = dbContext.Users.Cacheable().Single(u => u.Id == "test1");
            var single2 = dbContext.Users.Cacheable().Single(u => u.Id == "test2");
            Console.WriteLine($"Single 1: {single1.Id}, Single 2: {single2.Id}");
            Console.ReadKey();
        }
    }
}
