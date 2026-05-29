using Microsoft.Data.SqlClient;

namespace PremixOrderAPI.Data;

public class DatabaseConnection
{
    private readonly string _connectionString;
    public DatabaseConnection(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("PremixOrderDB")
            ?? configuration["ConnectionStrings:PremixOrderDB"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__PremixOrderDB")
            ?? throw new Exception("Connection string 'PremixOrderDB' not found.");
    }
    public SqlConnection CreateOpenConnection()
    {
        var conn = new SqlConnection(_connectionString);
        conn.Open();
        return conn;
    }
}