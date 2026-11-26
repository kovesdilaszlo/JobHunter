using JobHunter.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Test.Integration;

public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True");

        _connection.Open();

        using var context = CreateContext();

        context.Database.EnsureCreated();
    }

    public JobHunterDbContext CreateContext()
    {
        var options =
            new DbContextOptionsBuilder<JobHunterDbContext>()
                .UseSqlite(_connection)
                .Options;

        return new JobHunterDbContext(options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
