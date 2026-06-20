using CafeApp.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var argsMap = args
    .Select(arg => arg.Split('=', 2))
    .Where(parts => parts.Length == 2)
    .ToDictionary(parts => parts[0].TrimStart('-'), parts => parts[1]);

if (!argsMap.TryGetValue("server", out var server) ||
    !argsMap.TryGetValue("database", out var database))
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/DbAdmin/DbAdmin.csproj -- server=<server> database=<database> [identity-name=<managed-identity-name>] [identity-object-id=<object-id>] [db-user-name=<db-user-name>] [migrate=true]");
    return 1;
}

argsMap.TryGetValue("identity-name", out var identityName);
argsMap.TryGetValue("identity-object-id", out var identityObjectId);
var dbUserName = argsMap.TryGetValue("db-user-name", out var dbUserNameArg)
    ? dbUserNameArg
    : identityName;
var runMigrations = argsMap.TryGetValue("migrate", out var migrateArg) && bool.TryParse(migrateArg, out var migrate) && migrate;

if (!runMigrations && string.IsNullOrWhiteSpace(dbUserName))
{
    Console.Error.WriteLine("When migrate=false, specify identity-name and/or db-user-name.");
    return 1;
}

var connectionString = new SqlConnectionStringBuilder
{
    DataSource = server,
    InitialCatalog = database,
    Encrypt = true,
    TrustServerCertificate = false,
    ConnectTimeout = 30,
    Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault
}.ConnectionString;

await using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

if (runMigrations)
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    await using var db = new AppDbContext(options);
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine($"Database schema ensured for '{database}'.");
}

if (!string.IsNullOrWhiteSpace(dbUserName))
{
    var safeDbUserName = dbUserName.Replace("]", "]]", StringComparison.Ordinal);
    var escapedDbUserName = dbUserName.Replace("'", "''", StringComparison.Ordinal);
    var createUserCommand = string.IsNullOrWhiteSpace(identityObjectId)
        ? $"IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{escapedDbUserName}') CREATE USER [{safeDbUserName}] FROM EXTERNAL PROVIDER;"
        : $"IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{escapedDbUserName}') CREATE USER [{safeDbUserName}] FROM EXTERNAL PROVIDER WITH OBJECT_ID = '{identityObjectId}';";

    var commands = new[]
    {
        createUserCommand,
        $"ALTER ROLE db_datareader ADD MEMBER [{safeDbUserName}];",
        $"ALTER ROLE db_datawriter ADD MEMBER [{safeDbUserName}];"
    };

    foreach (var commandText in commands)
    {
        await using var command = new SqlCommand(commandText, connection);
        await command.ExecuteNonQueryAsync();
    }

    Console.WriteLine($"Database user ensured for managed identity '{dbUserName}' on '{database}'.");
}

return 0;