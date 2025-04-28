using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AdditionalInvImage")]
public class AdditionalInvImage
{
    [Key]
    public int ImageId { get; set; }
    public int ItemId { get; set; }
    public string ImageData { get; set; }

    public int? ImageSortOrder { get; set; } // Optional field for sorting
    public string ImageDescription { get; set; } // Optional field for description
}
