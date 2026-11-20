using System.Text.Json;
using HttpVerbsEfDemo.Data;
using HttpVerbsEfDemo.DTOs;
using HttpVerbsEfDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HttpVerbsEfDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    // GET: api/products - retrieve all products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return Ok(products);
    }

    // GET: api/products/1 - retrieve one product
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound(new { message = $"Product {id} was not found." }) : Ok(product);
    }

    // POST: api/products - create a new product
    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category,
            Price = dto.Price,
            Stock = dto.Stock
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT: api/products/1 - replace all editable fields
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Replace(int id, UpdateProductDto dto)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product {id} was not found." });

        product.Name = dto.Name;
        product.Category = dto.Category;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        await db.SaveChangesAsync();

        return Ok(product);
    }

    // PATCH: api/products/1 - update only fields supplied in the JSON body
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PartiallyUpdate(int id, [FromBody] JsonElement patch)
    {
        if (patch.ValueKind != JsonValueKind.Object)
            return BadRequest(new { message = "PATCH body must be a JSON object." });

        var product = await db.Products.FindAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product {id} was not found." });

        foreach (var property in patch.EnumerateObject())
        {
            switch (property.Name.ToLowerInvariant())
            {
                case "name":
                    if (property.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.Value.GetString()) || property.Value.GetString()!.Length > 100)
                        return BadRequest(new { message = "Name must be a non-empty string of at most 100 characters." });
                    product.Name = property.Value.GetString()!;
                    break;
                case "category":
                    if (property.Value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(property.Value.GetString()) || property.Value.GetString()!.Length > 60)
                        return BadRequest(new { message = "Category must be a non-empty string of at most 60 characters." });
                    product.Category = property.Value.GetString()!;
                    break;
                case "price":
                    if (!property.Value.TryGetDecimal(out var price) || price <= 0 || price > 10000000)
                        return BadRequest(new { message = "Price must be greater than 0 and at most 10000000." });
                    product.Price = price;
                    break;
                case "stock":
                    if (!property.Value.TryGetInt32(out var stock) || stock < 0 || stock > 100000)
                        return BadRequest(new { message = "Stock must be between 0 and 100000." });
                    product.Stock = stock;
                    break;
                default:
                    return BadRequest(new { message = $"'{property.Name}' is not a supported product field." });
            }
        }

        if (!patch.EnumerateObject().Any())
            return BadRequest(new { message = "Provide at least one field to update." });

        await db.SaveChangesAsync();
        return Ok(product);
    }

    // DELETE: api/products/1 - delete a product
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product {id} was not found." });

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // HEAD: api/products/1 - return status/headers without a response body
    [HttpHead("{id:int}")]
    public async Task<IActionResult> Head(int id)
    {
        var exists = await db.Products.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!exists)
            return NotFound();

        Response.Headers["X-Resource-Exists"] = "true";
        return Ok();
    }

    // OPTIONS: api/products - tell the client which methods this resource supports
    [HttpOptions]
    [HttpOptions("{id:int}")]
    public IActionResult Options()
    {
        Response.Headers["Allow"] = "GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS";
        return NoContent();
    }
}
