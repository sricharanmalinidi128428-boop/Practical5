# Practical 6 — Product Catalog

A clear, original ASP.NET Core MVC catalog that demonstrates the Practical 6 requirements: Models, Controllers, Views, and Routing.

## Run in Visual Studio

Open `Practical6.sln` with **File → Open → Project/Solution**. Select the **https** launch profile beside the green Run button, then press **F5**. The app opens at `https://localhost:7037` (HTTP fallback: `http://localhost:5050`).

## Included

- A `Product` model with ID, name, category, price, description, and validation annotations.
- `ProductsController` provides sample products, search and category filtering, and the product details action.
- Razor views display the catalog in a table and provide a separate product details page.
- Conventional MVC routing opens the catalog at `/` and product details at `/Products/Details/{id}`.

The catalog uses sample data stored in the controller; it does not require a database or contain checkout functionality.
