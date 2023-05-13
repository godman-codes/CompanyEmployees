using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace CompanyEmployees.ContextFactory
{
    public class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext>
    {
        // this clss gets the coonection string and points the 
        // the create db function to the migration assembly 
        public RepositoryContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<RepositoryContext>()
        .UseSqlServer(configuration.GetConnectionString("sqlConnection"),
        b => b.MigrationsAssembly("CompanyEmployees"));
            return new RepositoryContext(builder.Options);
        }
    }
}
