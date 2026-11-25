using System;
using System.Data.SqlClient;

internal class Program
{
    // SQL Server LocalDB connection strings.
    private const string MasterConnection = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Initial Catalog=master";
    private const string InventoryConnection = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Initial Catalog=InventoryDB";

    private static void Main()
    {
        try
        {
            CreateDatabaseAndTable();

            while (true)
            {
                Console.WriteLine("\n===== INVENTORY MANAGEMENT =====");
                Console.WriteLine("1. View all products");
                Console.WriteLine("2. Add a product");
                Console.WriteLine("3. Update a product");
                Console.WriteLine("4. Delete a product");
                Console.WriteLine("5. Exit");
                Console.Write("Choose an option: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewProducts(); break;
                    case "2": AddProduct(); break;
                    case "3": UpdateProduct(); break;
                    case "4": DeleteProduct(); break;
                    case "5": return;
                    default: Console.WriteLine("Please enter a number from 1 to 5."); break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not start the program: " + ex.Message);
            Console.WriteLine("Check that SQL Server LocalDB is installed and running.");
        }
    }

// Create the database and table automatically the first time the program runs.
    private static void CreateDatabaseAndTable()
    {
        using (var connection = new SqlConnection(MasterConnection))
        {
            connection.Open();
            using var command = new SqlCommand(
                "IF DB_ID('InventoryDB') IS NULL CREATE DATABASE InventoryDB;", connection);
            command.ExecuteNonQuery();
        }

        using (var connection = new SqlConnection(InventoryConnection))
        {
            connection.Open();
            using var command = new SqlCommand(@"
            IF OBJECT_ID('Products', 'U') IS NULL
            CREATE TABLE Products (
                ProductId INT IDENTITY(1,1) PRIMARY KEY,
                ProductName NVARCHAR(100) NOT NULL,
                Category NVARCHAR(50) NOT NULL,
                Quantity INT NOT NULL,
                Price DECIMAL(10,2) NOT NULL
            );", connection);
            command.ExecuteNonQuery();
        }
    }

// READ: show every product stored in the database.
    private static void ViewProducts()
    {
    using var connection = new SqlConnection(InventoryConnection);
    connection.Open();
    using var command = new SqlCommand(
        "SELECT ProductId, ProductName, Category, Quantity, Price FROM Products ORDER BY ProductId;", connection);
    using var reader = command.ExecuteReader();

    Console.WriteLine("\nID   Product              Category        Qty      Price");
    Console.WriteLine("----------------------------------------------------------");
    bool found = false;
    while (reader.Read())
    {
        found = true;
        Console.WriteLine($"{reader["ProductId"],-4} {reader["ProductName"],-20} {reader["Category"],-15} {reader["Quantity"],-8} {reader["Price"],8:C}");
    }
    if (!found) Console.WriteLine("No products yet. Choose 2 to add one.");
    }

// CREATE: insert one new product. Parameters keep user input separate from SQL.
    private static void AddProduct()
    {
    string name = ReadText("Product name: ");
    string category = ReadText("Category: ");
    int quantity = ReadNumber("Quantity: ");
    decimal price = ReadPrice("Price: ");

    using var connection = new SqlConnection(InventoryConnection);
    connection.Open();
    using var command = new SqlCommand(
        "INSERT INTO Products (ProductName, Category, Quantity, Price) VALUES (@name, @category, @quantity, @price);", connection);
    command.Parameters.AddWithValue("@name", name);
    command.Parameters.AddWithValue("@category", category);
    command.Parameters.AddWithValue("@quantity", quantity);
    command.Parameters.AddWithValue("@price", price);
    command.ExecuteNonQuery();
    Console.WriteLine("Product added successfully.");
    }

// UPDATE: change the details of the product with the given ID.
    private static void UpdateProduct()
    {
    int id = ReadNumber("Product ID to update: ");
    string name = ReadText("New product name: ");
    string category = ReadText("New category: ");
    int quantity = ReadNumber("New quantity: ");
    decimal price = ReadPrice("New price: ");

    using var connection = new SqlConnection(InventoryConnection);
    connection.Open();
    using var command = new SqlCommand(@"
        UPDATE Products
        SET ProductName = @name, Category = @category, Quantity = @quantity, Price = @price
        WHERE ProductId = @id;", connection);
    command.Parameters.AddWithValue("@name", name);
    command.Parameters.AddWithValue("@category", category);
    command.Parameters.AddWithValue("@quantity", quantity);
    command.Parameters.AddWithValue("@price", price);
    command.Parameters.AddWithValue("@id", id);
    Console.WriteLine(command.ExecuteNonQuery() == 1 ? "Product updated." : "Product ID not found.");
    }

// DELETE: remove the product with the given ID.
    private static void DeleteProduct()
    {
    int id = ReadNumber("Product ID to delete: ");
    using var connection = new SqlConnection(InventoryConnection);
    connection.Open();
    using var command = new SqlCommand("DELETE FROM Products WHERE ProductId = @id;", connection);
    command.Parameters.AddWithValue("@id", id);
    Console.WriteLine(command.ExecuteNonQuery() == 1 ? "Product deleted." : "Product ID not found.");
    }

    private static string ReadText(string prompt)
    {
    while (true)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        Console.WriteLine("This value cannot be empty.");
    }
    }

    private static int ReadNumber(string prompt)
    {
    while (true)
    {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int value) && value >= 0) return value;
        Console.WriteLine("Enter a whole number that is 0 or more.");
    }
    }

    private static decimal ReadPrice(string prompt)
    {
    while (true)
    {
        Console.Write(prompt);
        if (decimal.TryParse(Console.ReadLine(), out decimal value) && value >= 0) return value;
        Console.WriteLine("Enter a price that is 0 or more (example: 99.50).");
    }
    }
}
