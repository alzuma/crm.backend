using System.Data;
using Crm.Backend.Persistence.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Task = Crm.Backend.Persistence.Entities.Task;

namespace Crm.Backend.Persistence;

/// <summary>The CRM database context over SQLite. The six record types live
/// in the Entities folder, their column, constraint, relationship and index
/// configuration in one class per entity under Configurations.</summary>
public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
        // SQLite only enforces foreign keys per connection. Enable the
        // pragma every time one of this context's connections opens.
        Database.GetDbConnection().StateChange += EnableForeignKeys;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Task> Tasks => Set<Task>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);

    private void EnableForeignKeys(object? sender, EventArgs e)
    {
        if (sender is SqliteConnection { State: ConnectionState.Open } connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();
        }
    }
}
