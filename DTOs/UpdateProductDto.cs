using System.ComponentModel.DataAnnotations;

namespace HttpVerbsEfDemo.DTOs;

// PUT replaces all editable product fields, so all fields are required.
public class UpdateProductDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Category { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "10000000")]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }
}
