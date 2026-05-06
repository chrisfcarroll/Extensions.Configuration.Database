using System.Data;
using Microsoft.Extensions.Configuration;

namespace Extensions.Configuration.Database;

/// <summary>
/// An <see cref="IConfigurationProvider"/> backed by a table in a SQL database.
/// For usage in Program.cs see
/// <see cref="DatabaseConfigurationProviderExtensions.AddDatabaseTable"/>.
/// </summary>
/// <seealso href="https://gist.github.com/ErikNoren/5ab952ce93558ed1a79a97cd9a34bd37">
/// Original code from Erik Noren's gist
/// </seealso>
public class DatabaseConfigurationProvider : ConfigurationProvider, IDisposable
{
    readonly Timer? refreshTimer;

    public DatabaseConfigurationSourceSettings Source { get; }

    public DatabaseConfigurationProvider(DatabaseConfigurationSourceSettings source)
    {
        Source = source;

        if (Source.RefreshInterval.HasValue)
            refreshTimer = new Timer(_ => ReadDatabaseSettings(true), null, Timeout.Infinite, Timeout.Infinite);
    }

    public override void Load()
    {
        if (string.IsNullOrWhiteSpace(Source.ConnectionString))
            return;

        ReadDatabaseSettings(false);

        if (refreshTimer != null && Source.RefreshInterval.HasValue)
            refreshTimer.Change(Source.RefreshInterval.Value, Source.RefreshInterval.Value);
    }

    void ReadDatabaseSettings(bool isReload)
    {
        using var connection = DbProviders.CreateConnection(Source.ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = 
            $"SELECT {Source.KeyColumnName}, {Source.ValueColumnName} FROM {Source.TableName} WHERE {Source.WhereClause}";

        try
        {
            connection.Open();
            var reader = command.ExecuteReader();

            var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            while(reader.Read())
            {
                try
                {
                    settings[reader.GetString(0)] = reader.GetString(1);
                }
                catch (Exception readerEx)
                {
                    System.Diagnostics.Debug.WriteLine(readerEx);
                }
            }

            reader.Close();

            if (!isReload || !SettingsMatch(Data, settings))
            {
                Data = settings;

                if (isReload)
                    OnReload();
            }
        }
        catch (Exception sqlEx)
        {
            System.Diagnostics.Debug.WriteLine(sqlEx);
        }
    }

    bool SettingsMatch(IDictionary<string, string?> oldSettings, IDictionary<string, string?> newSettings)
    {
        if (oldSettings.Count != newSettings.Count)
            return false;

        return oldSettings
            .OrderBy(s => s.Key)
            // ReSharper disable UsageOfDefaultStructEquality we only have a handful of rows
            .SequenceEqual(newSettings.OrderBy(s => s.Key));
            // ReSharper restore UsageOfDefaultStructEquality
    }

    public void Dispose()
    {
        refreshTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        refreshTimer?.Dispose();
    }
}
