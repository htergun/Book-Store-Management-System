# BookStoreManagement

Student database project: .NET 10 console application + Microsoft SQL Server (database name: BookStoreDB).

## How to run
1. Open SQL Server Management Studio (SSMS), connect to your server and run `Database/BookStoreDB.sql`
   (creates the database `BookStoreDB`, 5 tables and sample data).
2. Open `BookStoreManagement.csproj` in Visual Studio (File > Open > Project/Solution). Requires the .NET 10 SDK.
3. In `Program.cs`, check the `ConnectionString`. Default is LocalDB: `(localdb)\MSSQLLocalDB`.
   For SQL Server Express use `.\SQLEXPRESS`; for a full instance use your server name.
4. Press F5. NuGet restores `Microsoft.Data.SqlClient` automatically.

`Database/Queries.sql` contains the 8 information retrieval queries used in the report.
