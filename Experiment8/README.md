# Experiment 8: Inventory Management using ADO.NET

## Aim
Build a small inventory program that performs Create, Read, Update, and Delete (CRUD) operations with ADO.NET and SQL Server LocalDB.

## Requirements
- Visual Studio with the .NET Framework 4.8 Developer Pack
- SQL Server Express LocalDB (usually included with Visual Studio)
- No extra NuGet package is required; the project uses .NET Framework's built-in SQL client.

## Run in Visual Studio
1. Open `InventoryManagement.csproj` in Visual Studio.
2. Let Visual Studio load the project.
3. Press **Ctrl+F5** (or click **Start Without Debugging**).
4. Choose menu options 1–5. The program creates `InventoryDB` and its `Products` table automatically on first run.

## ADO.NET concepts used
- `System.Data.SqlClient.SqlConnection` connects the C# program to SQL Server.
- `SqlCommand` runs SQL statements.
- `SqlDataReader` reads rows returned by `SELECT`.
- `ExecuteNonQuery()` runs `INSERT`, `UPDATE`, and `DELETE` statements.
- SQL parameters such as `@name` safely pass user input into a query. This classic .NET Framework project uses the built-in `System.Data.SqlClient`, so it needs no extra NuGet package.

## Result
Products can be added, listed, edited, and deleted. Their data remains in the local `InventoryDB` database after the program closes.
