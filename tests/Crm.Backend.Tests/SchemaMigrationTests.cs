using Crm.Backend.Persistence;
using Crm.Backend.Persistence.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Task = Crm.Backend.Persistence.Entities.Task;

namespace Crm.Backend.Tests;

/// <summary>Schema tests: apply the real migrations to a throwaway SQLite
/// database and verify the tables, columns, indexes and constraints behave
/// as the data model requires. Each test gets its own file database.</summary>
public sealed class SchemaMigrationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly CrmDbContext _context;

    public SchemaMigrationTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"crm7-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        _context = new CrmDbContext(options);
        _context.Database.Migrate();
    }

    public void Dispose()
    {
        _context.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [Fact]
    public void Migrate_CreatesAllSixTables_WithExpectedColumns()
    {
        var audit = new[] { "CreatedAtUtc", "UpdatedAtUtc", "CreatedBy", "UpdatedBy" };
        var expected = new Dictionary<string, string[]>
        {
            ["Users"] = new[] { "Id", "Email", "NormalizedEmail", "PasswordHash", "DisplayName", "Role", "IsActive" }.Concat(audit).ToArray(),
            ["Companies"] = new[] { "Id", "Name", "Website", "Phone", "Industry", "Status", "Address", "Notes" }.Concat(audit).ToArray(),
            ["Contacts"] = new[] { "Id", "CompanyId", "FirstName", "LastName", "Email", "Phone", "JobTitle", "Notes" }.Concat(audit).ToArray(),
            ["Opportunities"] = new[] { "Id", "CompanyId", "OwnerId", "Title", "AmountMinor", "Currency", "Stage", "Probability", "ExpectedCloseDate", "LossReason" }.Concat(audit).ToArray(),
            ["Activities"] = new[] { "Id", "CompanyId", "ContactId", "OpportunityId", "Type", "OccurredAtUtc", "Summary", "Notes" }.Concat(audit).ToArray(),
            ["Tasks"] = new[] { "Id", "CompanyId", "OpportunityId", "AssignedTo", "Title", "Description", "DueAt", "Priority", "Status" }.Concat(audit).ToArray(),
        };

        foreach (var (table, columns) in expected)
        {
            var actual = ColumnsOf(table).Select(column => column.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.Equal(new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase), actual);
        }
    }

    [Fact]
    public void AllTables_UseUuidTextPrimaryKeys()
    {
        foreach (var table in new[] { "Users", "Companies", "Contacts", "Opportunities", "Activities", "Tasks" })
        {
            var idType = ColumnsOf(table).Single(column => column.Name == "Id").Type;
            Assert.Equal("TEXT", idType);
        }
    }

    [Fact]
    public void Dates_AreStoredAsUtcText()
    {
        var company = new Company
        {
            Name = "Acme",
            CreatedAtUtc = new DateTime(2026, 1, 2, 3, 4, 5, 0, DateTimeKind.Utc),
        };
        _context.Add(company);
        _context.SaveChanges();

        using var connection = _context.Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CreatedAtUtc FROM Companies LIMIT 1;";
        var stored = (string)command.ExecuteScalar()!;

        Assert.StartsWith("2026-01-02 03:04:05", stored);
        Assert.Matches(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}(\.\d+)?$", stored);
    }

    [Fact]
    public void Migrate_IsIdempotent()
    {
        _context.Database.Migrate();
        _context.Database.Migrate();
    }

    [Fact]
    public void Users_UniqueNormalizedEmailIndex_Exists()
    {
        var index = IndexesOf("Users").Single(index => index.Name == "IX_Users_NormalizedEmail");
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void ExpectedIndexes_AllExist()
    {
        var expected = new Dictionary<string, string[]>
        {
            ["Companies"] = new[] { "IX_Companies_Status" },
            ["Contacts"] = new[] { "IX_Contacts_CompanyId" },
            ["Opportunities"] = new[] { "IX_Opportunities_CompanyId", "IX_Opportunities_OwnerId", "IX_Opportunities_Stage" },
            ["Activities"] = new[] { "IX_Activities_CompanyId", "IX_Activities_ContactId", "IX_Activities_OpportunityId", "IX_Activities_Type" },
            ["Tasks"] = new[] { "IX_Tasks_CompanyId", "IX_Tasks_OpportunityId", "IX_Tasks_AssignedTo", "IX_Tasks_Status", "IX_Tasks_DueAt" },
        };

        foreach (var (table, names) in expected)
        {
            var actual = IndexesOf(table).Select(index => index.Name).ToList();
            foreach (var name in names)
            {
                Assert.Contains(name, actual);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Companies_EmptyOrWhitespaceName_RejectedByDatabase(string name)
    {
        AssertRejectedByDatabase(() => Add(new Company { Name = name }));
    }

    [Fact]
    public void Companies_ValidName_Persists()
    {
        var company = Add(new Company { Name = "Acme" });
        Assert.Equal("Acme", _context.Companies.Find(company.Id)?.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void Opportunities_EmptyOrWhitespaceTitle_RejectedByDatabase(string title)
    {
        var (company, user) = SeedCompanyAndUser();
        AssertRejectedByDatabase(() => Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = user.Id,
            Title = title,
            Stage = OpportunityStage.New,
            Probability = 10,
        }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Opportunities_ProbabilityOutside0To100_RejectedByDatabase(int probability)
    {
        var (company, user) = SeedCompanyAndUser();
        AssertRejectedByDatabase(() => Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = user.Id,
            Title = "Deal",
            Stage = OpportunityStage.New,
            Probability = probability,
        }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Opportunities_ProbabilityAtBoundaries_Persists(int probability)
    {
        var (company, user) = SeedCompanyAndUser();
        Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = user.Id,
            Title = "Deal",
            Stage = OpportunityStage.New,
            Probability = probability,
        });
    }

    [Fact]
    public void Opportunities_NegativeAmount_RejectedByDatabase()
    {
        var (company, user) = SeedCompanyAndUser();
        AssertRejectedByDatabase(() => Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = user.Id,
            Title = "Deal",
            Stage = OpportunityStage.New,
            Probability = 10,
            AmountMinor = -1,
        }));
    }

    [Fact]
    public void Users_DuplicateEmail_DifferingCaseAndWhitespace_RejectedByDatabase()
    {
        Add(new User { Email = "a@Example.com", PasswordHash = "hash", DisplayName = "A" });
        AssertRejectedByDatabase(() =>
            Add(new User { Email = " A@example.COM ", PasswordHash = "hash", DisplayName = "B" }));
    }

    [Fact]
    public void Users_DuplicateIdenticalEmail_RejectedByDatabase()
    {
        Add(new User { Email = "same@example.com", PasswordHash = "hash", DisplayName = "A" });
        AssertRejectedByDatabase(() =>
            Add(new User { Email = "same@example.com", PasswordHash = "hash", DisplayName = "B" }));
    }

    [Fact]
    public void Contacts_UnknownCompany_RejectedByDatabase()
    {
        AssertRejectedByDatabase(() => Add(new Contact { CompanyId = Guid.NewGuid() }));
    }

    [Fact]
    public void Opportunities_UnknownOwner_RejectedByDatabase()
    {
        var company = Add(new Company { Name = "Acme" });
        AssertRejectedByDatabase(() => Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = Guid.NewGuid(),
            Title = "Deal",
            Stage = OpportunityStage.New,
            Probability = 10,
        }));
    }

    [Fact]
    public void Tasks_UnknownAssignee_RejectedByDatabase()
    {
        AssertRejectedByDatabase(() => Add(new Task { AssignedTo = Guid.NewGuid(), Title = "Call" }));
    }

    [Fact]
    public void Tasks_WithoutCompanyOrOpportunity_Persist()
    {
        var user = Add(new User { Email = $"user-{Guid.NewGuid():N}@example.com", PasswordHash = "hash", DisplayName = "U" });
        var task = Add(new Task { AssignedTo = user.Id, Title = "Call" });
        Assert.NotNull(_context.Tasks.Find(task.Id));
    }

    [Fact]
    public void Activities_UnknownCompany_RejectedByDatabase()
    {
        AssertRejectedByDatabase(() => Add(new Activity
        {
            CompanyId = Guid.NewGuid(),
            Type = "call",
            OccurredAtUtc = DateTime.UtcNow,
        }));
    }

    [Fact]
    public void Activities_UnknownOpportunity_RejectedByDatabase()
    {
        var company = Add(new Company { Name = "Acme" });
        AssertRejectedByDatabase(() => Add(new Activity
        {
            CompanyId = company.Id,
            OpportunityId = Guid.NewGuid(),
            Type = "call",
            OccurredAtUtc = DateTime.UtcNow,
        }));
    }

    [Fact]
    public void Activities_WithoutContactOrOpportunity_Persist()
    {
        var company = Add(new Company { Name = "Acme" });
        var activity = Add(new Activity
        {
            CompanyId = company.Id,
            Type = "call",
            OccurredAtUtc = DateTime.UtcNow,
        });
        Assert.NotNull(_context.Activities.Find(activity.Id));
    }

    [Fact]
    public void Companies_WithDependents_CannotBeDeleted()
    {
        var (company, user) = SeedCompanyAndUser();
        Add(new Contact { CompanyId = company.Id });
        Add(new Opportunity
        {
            CompanyId = company.Id,
            OwnerId = user.Id,
            Title = "Deal",
            Stage = OpportunityStage.New,
            Probability = 10,
        });
        Add(new Activity { CompanyId = company.Id, Type = "call", OccurredAtUtc = DateTime.UtcNow });
        Add(new Task { CompanyId = company.Id, AssignedTo = user.Id, Title = "Call" });

        // A fresh context deletes the company without tracking its dependents,
        // so the delete reaches SQLite and the foreign keys block it there.
        AssertRejectedByDatabase(() =>
        {
            using var deleteContext = CreateContext();
            var tracked = deleteContext.Companies.Find(company.Id)!;
            deleteContext.Companies.Remove(tracked);
            deleteContext.SaveChanges();
        });
    }

    [Fact]
    public void Companies_WithoutDependents_CanBeDeleted()
    {
        var company = Add(new Company { Name = "Acme" });
        _context.Companies.Remove(company);
        _context.SaveChanges();
        Assert.Null(_context.Companies.Find(company.Id));
    }

    // --- helpers -----------------------------------------------------------

    private CrmDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options);

    private T Add<T>(T entity)
        where T : class
    {
        if (entity is User user)
        {
            user.CreatedAtUtc = DateTime.UtcNow;
        }
        else if (entity is Company company)
        {
            company.CreatedAtUtc = DateTime.UtcNow;
        }
        else if (entity is Contact contact)
        {
            contact.CreatedAtUtc = DateTime.UtcNow;
        }
        else if (entity is Opportunity opportunity)
        {
            opportunity.CreatedAtUtc = DateTime.UtcNow;
        }
        else if (entity is Activity activity)
        {
            activity.CreatedAtUtc = DateTime.UtcNow;
        }
        else if (entity is Task task)
        {
            task.CreatedAtUtc = DateTime.UtcNow;
        }

        _context.Add(entity);
        _context.SaveChanges();
        return entity;
    }

    private (Company Company, User User) SeedCompanyAndUser()
    {
        var company = Add(new Company { Name = "Acme" });
        var user = Add(new User { Email = $"owner-{Guid.NewGuid():N}@example.com", PasswordHash = "hash", DisplayName = "Owner" });
        return (company, user);
    }

    /// <summary>Constraint violations surface as SqliteException, or as
    /// DbUpdateException wrapping one, depending on the EF version.</summary>
    private static void AssertRejectedByDatabase(Action action)
    {
        var exception = Record.Exception(action);
        Assert.NotNull(exception);
        Assert.True(
            exception is SqliteException or DbUpdateException,
            $"Expected a database constraint violation, got {exception!.GetType().Name}: {exception.Message}");
    }

    private HashSet<string> SetOf(string sql)
        => _context.Database
            .SqlQueryRaw<string>(sql)
            .AsEnumerable()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private readonly record struct ColumnInfo(string Name, string Type);

    // table_xinfo (unlike table_info) also lists generated columns such as
    // Users.NormalizedEmail.
    private List<ColumnInfo> ColumnsOf(string table)
    {
        using var connection = _context.Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_xinfo(\"{table}\");";
        using var reader = command.ExecuteReader();
        var columns = new List<ColumnInfo>();
        while (reader.Read())
        {
            columns.Add(new ColumnInfo(reader.GetString(1), reader.GetString(2)));
        }

        return columns;
    }

    private readonly record struct IndexInfo(string Name, bool IsUnique);

    private List<IndexInfo> IndexesOf(string table)
    {
        using var connection = _context.Database.GetDbConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA index_list(\"{table}\");";
        using var reader = command.ExecuteReader();
        var indexes = new List<IndexInfo>();
        while (reader.Read())
        {
            indexes.Add(new IndexInfo(reader.GetString(1), reader.GetInt32(2) == 1));
        }

        return indexes;
    }
}
