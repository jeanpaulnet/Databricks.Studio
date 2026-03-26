using Databricks.Studio.Entity.Data;
using Microsoft.EntityFrameworkCore;

namespace Databricks.Studio.UnitTests.Fixtures;

public class DbContextFixture : IDisposable
{
    public StudioDbContext Context { get; }

    public DbContextFixture()
    {
        var options = new DbContextOptionsBuilder<StudioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new StudioDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
