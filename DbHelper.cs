using Microsoft.Data.SqlClient;

namespace BookStoreManagement;

// Bütün formlar veritabanına bu sınıf üzerinden bağlanacak.
public static class DbHelper
{
    // Sunucu adını SSMS'te bağlanırken yazdığınla aynı yap:
    //   SQL Server Express : localhost\SQLEXPRESS
    //   LocalDB            : (localdb)\MSSQLLocalDB
    private const string ConnectionString =
        @"Server=DESKTOP-GEDUPJI;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // Her işlem için yeni bağlantı üretir (çağıran taraf using ile kapatır).
    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }

    // Bağlantı çalışıyor mu diye test eder.
    public static bool TestConnection(out string error)
    {
        try
        {
            using var connection = GetConnection();
            connection.Open();
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}