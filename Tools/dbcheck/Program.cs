using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

var connStr = "Server=localhost,1433;Database=CareeraDB;User Id=sa;Password=Underworld@23952801;TrustServerCertificate=True;";
int paymentId = 1043;

try
{
    using var conn = new SqlConnection(connStr);
    await conn.OpenAsync();

    var query = @"SELECT p.Id AS PaymentId, p.UserId, u.RoleId, r.Name AS RoleName
FROM dbo.PaymentTransactions p
JOIN dbo.Users u ON p.UserId = u.Id
LEFT JOIN dbo.Roles r ON u.RoleId = r.Id
WHERE p.Id = @paymentId;";

    using var cmd = conn.CreateCommand();
    cmd.CommandText = query;
    cmd.Parameters.Add(new SqlParameter("@paymentId", SqlDbType.Int) { Value = paymentId });

    using var reader = await cmd.ExecuteReaderAsync();

    var results = new System.Collections.Generic.List<object>();
    while (await reader.ReadAsync())
    {
        results.Add(new {
            PaymentId = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            RoleId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
            RoleName = reader.IsDBNull(3) ? null : reader.GetString(3)
        });
    }

    Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions{WriteIndented=true}));
    return 0;
}
catch(Exception ex)
{
    Console.Error.WriteLine("ERROR: " + ex.Message);
    Console.Error.WriteLine(ex.StackTrace);
    return 2;
}
