using System.Data;
using System.Data.Common;

namespace Extensions.Configuration.Database;

/// <summary>
/// Recognise well-known databases from their connection strings, and
/// create an IDbConnection for the specified provider.
/// </summary>
public static class DbProviders
{
    /// <summary>Provider name for PostgreSQL using Npgsql</summary>
    public const string PostgresNpgsql = "Npgsql";
    
    /// <summary>Provider name for MySQl using MySqlConnector</summary>
    public const string MySqlConnector = "MySqlConnector";
    
    /// <summary>Provider name for Sqlite using Microsoft.Data.Sqlite</summary>
    public const string Sqlite = "Microsoft.Data.Sqlite";
    
    /// <summary>Provider name for MS SQL Server using Microsoft.Data.SqlClient</summary>
    public const string MsSqlServer = "Microsoft.Data.SqlClient";
    
    /// <summary>Provider name for Oracle using Oracle.ManagedDataAccess.Client</summary>
    public const string Oracle = "Oracle.ManagedDataAccess.Client";


    public static IDbConnection CreateConnection(string connectionString, string? providerName=null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Empty connectionstring", nameof(connectionString));
        
        providerName ??= DeduceProviderName(connectionString);
        var factory = DbProviderFactories.GetFactory(providerName);
        var connection = factory.CreateConnection()
                         ?? throw new InvalidOperationException("Failed to create connection");
        connection.ConnectionString = connectionString;
        return connection;
    }
    
    static string DeduceProviderName(string connectionString)
    {
        connectionString = connectionString.ToLowerInvariant();
        var keys = GetConnectionStringKeys(connectionString);

        if (keys.Contains("host"))
            return PostgresNpgsql;

        else if (keys.Contains("service_name") ||
                 keys.Contains("sid") ||
                 connectionString.Contains("Data Source=(DESCRIPTION"))
            return Oracle;

        else if (keys.Contains("initial catalog") ||
                 keys.Contains("integrated security") ||
                 keys.Contains("trustservercertificate"))
            return MsSqlServer;
        

        else if (keys.Contains("data source") && !keys.Contains("initial catalog"))
            return Sqlite;
        
        else if (keys.Contains("uid") || keys.Contains("user id") && keys.Contains("port"))
            return MySqlConnector;


        throw new NotSupportedException(
            "Unable to deduce database provider from connection string.");
    }

    #if NET5_0_OR_GREATER
    static HashSet<string> GetConnectionStringKeys(string connectionString) 
        => connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', StringSplitOptions.TrimEntries)[0]
                           .Trim().ToLowerInvariant())
            .ToHashSet();
    #else
    static List<string> GetConnectionStringKeys(string connectionString)
        => connectionString
            .Split(';')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Select(p => p.Split('=')[0]
                        .Trim()
                        .ToLowerInvariant())
            .Distinct()
            .ToList();
    #endif
}