using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Database;

/// <summary>
/// Extensions to add MS SQL Server configuration sources to IConfigurationBuilder instances.
/// </summary>
/// <seealso href="https://gist.github.com/ErikNoren/5ab952ce93558ed1a79a97cd9a34bd37">
/// Original code from Erik Noren's gist
/// </seealso>
public static class DatabaseConfigurationProviderExtensions
{
    /// <summary>
    /// Add an MS SQL Server source to the configuration builder.
    /// Usage
    /// <code>
    /// builder.Configuration.AddSqlDatabase(config =>
    ///     {
    ///         //We can get the connection string from previously added ConfigurationProviders to use in setting this up
    ///         config.ConnectionString = builder.Configuration.GetConnectionString("DemoDatabase");
    ///         config.RefreshInterval = TimeSpan.FromMinutes(1);
    ///     });
    /// //Settings from all sources will be merged together. Since the database provider is added after the default
    /// //providers it can be used to override settings from those other providers.
    /// builder.Services.Configure&lt;Settings&gt;(builder.Configuration);
    /// </code>
    /// </summary>
    /// <param name="builder">The Microsoft.Extensions.Configuration.IConfigurationBuilder instance</param>
    /// <param name="configurationSource">
    /// An Action to configure the SQL database configuration source. Use this to set
    /// connection string and refresh interval on <see cref="DatabaseConfigurationSourceSettings"/>
    /// </param>
    /// <returns><paramref name="builder"/></returns>
    public static IConfigurationBuilder AddDatabaseTable(this IConfigurationBuilder builder, Action<DatabaseConfigurationSourceSettings>? configurationSource)
        => builder.Add(configurationSource);
}