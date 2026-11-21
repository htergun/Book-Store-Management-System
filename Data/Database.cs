using System.Data;
using Microsoft.Data.SqlClient;

namespace BookStoreManagement.Data;

// Common database helper: every repository uses these methods.
public static class Database
{
    // Change the server name if you use SQL Server Express (e.g. .\SQLEXPRESS)
    public const string ConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;" +
        "Database=BookStoreDB;" +
        "Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    // creates a parameter and converts null to DBNull
    public static SqlParameter Param(string name, object? value)
    {
        return new SqlParameter(name, value ?? DBNull.Value);
    }

    // SELECT -> DataTable
    public static DataTable GetTable(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection conn = CreateConnection();
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
        DataTable table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    // INSERT / UPDATE / DELETE -> affected rows
    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection conn = CreateConnection();
        conn.Open();
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteNonQuery();
    }

    // SELECT that returns one value (COUNT, SUM, ...)
    public static object? Scalar(string sql, params SqlParameter[] parameters)
    {
        using SqlConnection conn = CreateConnection();
        conn.Open();
        using SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters);
        return cmd.ExecuteScalar();
    }
}
