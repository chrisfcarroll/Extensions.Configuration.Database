using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Database;

/// <summary>
/// Settings for a <see cref="DatabaseConfigurationProvider"/>.
/// </summary>
public record DatabaseConfigurationSourceSettings : IConfigurationSource
{
    /// <summary>Required. The connectionstring to the database.</summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Required. The Table Name (possibly including schema name) from which to read settings.
    /// </summary>
    public string TableName { get; set; } = null!;

    /// <summary>Defaults to "Key"</summary>
    public string KeyColumnName { get; set; } = "Key";

    /// <summary>Defaults to "Value"</summary>
    public string ValueColumnName { get; set; } = "Value";


    /// <summary>
    /// Optional. Filter the rows retried.
    /// <b>Only strings of the form <c>identifier =|&lt;|&gt;|!= value</c> are accepted.</b>,
    /// for instance, Archived=0
    /// </summary>
    public string? WhereClause { get; set; }

    public TimeSpan? RefreshInterval { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new global::Extensions.Configuration.Database.DatabaseConfigurationProvider(this);

    /// <summary>
    /// Validates this DatabaseConfigurationSourceSettings.
    /// </summary>
    /// <returns>
    /// </returns>
    public BoolBecause Validate()
        => new[]
        {
            BoolBecause.Requires(ConnectionString is not null),
            BoolBecause.Requires(TableName is not null),
            BoolBecause.Requires(
                SimpleSqlClauseValidator.IsASimpleIdentifierOperatorValueExpression(WhereClause.AsSpan()))
        }.Aggregate((l, r) => l && r);

}