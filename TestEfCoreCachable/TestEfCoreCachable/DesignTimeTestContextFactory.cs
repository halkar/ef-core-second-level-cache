using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Reflection;

namespace TestEfCoreCachable
{
    public class DesignTimeTestContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>();

            builder.UseSqlServer("Data Source=.;Initial Catalog=TestDb;Integrated security=True;MultipleActiveResultSets=True", sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(TestDbContext).GetTypeInfo().Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });

            return new TestDbContext(builder.Options);
        }
    }
}