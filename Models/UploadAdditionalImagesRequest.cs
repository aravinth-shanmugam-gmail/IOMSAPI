public class UploadAdditionalImagesRequest
{
    public List<IFormFile> Images { get; set; }
    public List<int?> ImageSortOrders { get; set; }
    public List<string> ImageDescriptions { get; set; }
}
