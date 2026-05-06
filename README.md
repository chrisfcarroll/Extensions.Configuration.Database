# Extensions.Configuration.Database

A DatabaseConfigurationProvider for Microsoft.Extensions.Configuration.

For instance, in Program.cs

```csharp
    return Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true);
            config.AddDatabaseTable(o =>
            {
                o.ConnectionString
                    = context.Configuration.GetConnectionString("MyApplicationDatabase") ??
                      throw new ConfigurationException("ConnectionStrings:MyApplicationDatabase is missing");
                o.TableName = "dbo.Settings";
            });
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);
        })
        ...
        .Build();
```

#### References
- [Microsoft.Extensions.Configuration](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration)
- [GitHub Repo](https://github.com/chrisfcarroll/Extensions.Configuration.Database)
- [Based On](https://gist.github.com/ErikNoren/5ab952ce93558ed1a79a97cd9a34bd37)
