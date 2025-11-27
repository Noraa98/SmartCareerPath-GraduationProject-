using System;
using Microsoft.Data.SqlClient;

var conn = Environment.GetEnvironmentVariable("DB_CONN")
    ?? "Server=localhost,1433;Database=CareeraDB;User Id=sa;Password=Underworld@23952801;TrustServerCertificate=True;Connection Timeout=30";

Console.WriteLine("Connecting to DB...");

try
{
    using var connection = new SqlConnection(conn);
    await connection.OpenAsync();

    var cmdText = @"IF EXISTS(SELECT * FROM sys.columns WHERE [object_id]=OBJECT_ID(N'[dbo].[AuthTokens]') AND name='Token' AND (max_length < 0 OR max_length < 2000))
BEGIN
    ALTER TABLE [dbo].[AuthTokens] ALTER COLUMN [Token] NVARCHAR(MAX) NOT NULL;
    PRINT 'Column altered to NVARCHAR(MAX)';
END
ELSE
BEGIN
    PRINT 'Column already NVARCHAR(MAX) or alteration not required.';
END";

    using var command = new SqlCommand(cmdText, connection);
    command.CommandTimeout = 60;
    var reader = await command.ExecuteReaderAsync();
    do
    {
        while (await reader.ReadAsync())
        {
            Console.WriteLine(reader.GetValue(0));
        }
    } while (await reader.NextResultAsync());

    Console.WriteLine("Done.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error running SQL: {ex}");
    Environment.ExitCode = 2;
}
