
using System.ComponentModel.DataAnnotations.Schema;

[Table("InventoryItem")]
public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Unit { get; set; }
    public int MinUnit { get; set; }
    public decimal PricePerUnit { get; set; }
    public string? ImageFilePath { get; set; } 
}
