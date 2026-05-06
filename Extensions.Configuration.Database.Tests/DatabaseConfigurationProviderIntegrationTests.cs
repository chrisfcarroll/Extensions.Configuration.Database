using System.Data.Common;
using Microsoft.Data.Sqlite;
using TestBase;

namespace Extensions.Configuration.Database.Tests;

[TestClass]
public class DatabaseConfigurationProviderIntegrationTests
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        #if !NETFRAMEWORK && !NETSTANDARD
        // Register the Sqlite provider factory if it's not already registered
        try
        {
            DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", SqliteFactory.Instance);
        }
        catch (ArgumentException)
        {
            // Already registered
        }
        #endif
    }

    [TestMethod]
    public void Load_ReadsSettingsFromDatabase()
    {
        // Arrange
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE Settings (Key TEXT, Value TEXT)";
            command.ExecuteNonQuery();
            
            command.CommandText = "INSERT INTO Settings (Key, Value) VALUES ('Setting1', 'Value1'), ('Setting2', 'Value2')";
            command.ExecuteNonQuery();
        }

        var source = new DatabaseConfigurationSourceSettings
        {
            ConnectionString = connection.ConnectionString,
            TableName = "Settings",
            KeyColumnName = "Key",
            ValueColumnName = "Value",
            WhereClause = "1=1"
        };

        var provider = new DatabaseConfigurationProvider(source);

        // Act
        provider.Load();

        // Assert
        provider.TryGet("Setting1", out var value1).ShouldBeTrue();
        value1.ShouldBe("Value1");
        provider.TryGet("Setting2", out var value2).ShouldBeTrue();
        value2.ShouldBe("Value2");
    }

    [TestMethod]
    public void Load_WithWhereClause_FiltersResults()
    {
        // Arrange
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE Settings (Key TEXT, Value TEXT, Env TEXT)";
            command.ExecuteNonQuery();
            
            command.CommandText = "INSERT INTO Settings (Key, Value, Env) VALUES ('S1', 'V1', 'Prod'), ('S2', 'V2', 'Dev')";
            command.ExecuteNonQuery();
        }

        var source = new DatabaseConfigurationSourceSettings
        {
            ConnectionString = connection.ConnectionString,
            TableName = "Settings",
            KeyColumnName = "Key",
            ValueColumnName = "Value",
            WhereClause = "Env='Prod'"
        };

        var provider = new DatabaseConfigurationProvider(source);

        // Act
        provider.Load();

        // Assert
        provider.TryGet("S1", out var v1).ShouldBeTrue();
        v1.ShouldBe("V1");
        provider.TryGet("S2", out _).ShouldBeFalse();
    }
}
