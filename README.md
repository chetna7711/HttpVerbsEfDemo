# HTTP Verbs + EF Core Demo (.NET 10)

A small ASP.NET Core Web API demonstrating GET, POST, PUT, PATCH, DELETE, HEAD and OPTIONS with Entity Framework Core + SQL Server.

## Requirements
- .NET 10 SDK
- SQL Server LocalDB (or change the connection string to your SQL Server instance)
- Visual Studio 2026 / VS Code (optional)

## 1. Configure the database
The default connection string in `appsettings.json` uses Windows LocalDB:

`Server=(localdb)\MSSQLLocalDB;Database=HttpVerbsEfDemoDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

If you use another SQL Server instance, update `ConnectionStrings:DefaultConnection`.

## 2. Create the database using EF Core migrations
Open a terminal in this project folder and run:

```bash
dotnet restore
dotnet tool install --global dotnet-ef --version 10.0.0
```

If `dotnet-ef` is already installed, update it if needed:

```bash
dotnet tool update --global dotnet-ef --version 10.0.0
```

Then create and apply the migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 3. Run the API

```bash
dotnet run
```

Open the Swagger URL printed in the terminal (the launch profile normally opens `https://localhost:7147/swagger`).

## 4. Test endpoints
Base URL: `https://localhost:7147`

### GET all products
`GET /api/products`

Returns all products. Initially the list is empty.

### POST create a product
`POST /api/products`

Header: `Content-Type: application/json`

```json
{
  "name": "Laptop",
  "category": "Electronics",
  "price": 50000,
  "stock": 10
}
```

Expected: `201 Created`, with the created product and a Location header.

### GET one product
`GET /api/products/1`

Expected: `200 OK` if found; otherwise `404 Not Found`.

### PUT replace a product
`PUT /api/products/1`

Send all editable fields, because PUT replaces the complete editable representation:

```json
{
  "name": "Laptop Pro",
  "category": "Electronics",
  "price": 65000,
  "stock": 8
}
```

### PATCH only selected fields
`PATCH /api/products/1`

Send only the fields you want to change:

```json
{
  "price": 62000
}
```

Try changing only `stock` next. Unspecified fields remain unchanged. Supported fields: `name`, `category`, `price`, `stock`.

### HEAD check resource existence
`HEAD /api/products/1`

Returns status and headers without a response body. Check the status code and `X-Resource-Exists` response header. A missing product returns `404`.

### OPTIONS discover supported methods
`OPTIONS /api/products` or `OPTIONS /api/products/1`

Returns `204 No Content` and an `Allow` response header listing supported methods.

### DELETE remove a product
`DELETE /api/products/1`

Returns `204 No Content` when deleted; `404` if it does not exist.

## 5. Suggested testing order
1. GET all (empty list initially).
2. POST a product.
3. GET all and GET by ID.
4. HEAD the existing ID, then try a missing ID.
5. OPTIONS and inspect the `Allow` header.
6. PUT and send all fields.
7. PATCH and send only one field; verify other fields remain unchanged.
8. DELETE the product.
9. GET the deleted ID and observe `404`.

## Important notes
- `SaveChangesAsync()` persists inserts, updates and deletes to SQL Server.
- GET uses `AsNoTracking()` because it only reads data.
- PUT updates all editable fields; PATCH updates only supplied fields.
- HEAD is like GET for checking status/headers, but has no response body.
- OPTIONS advertises supported methods. It is different from CORS configuration; browser CORS preflight also uses OPTIONS and is handled by CORS middleware when configured.
- The API does not include TRACE or CONNECT endpoints because they are generally not application CRUD operations. TRACE is commonly disabled for security; CONNECT is normally handled by proxies for tunnelling.
