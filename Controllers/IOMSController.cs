using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;


namespace IOMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IOMSContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "images";
        public InventoryController(IOMSContext context, IConfiguration configuration)
        {
            _context = context;
            _blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage"));
        }

        [HttpPost("UploadAdditionalImages/{id}")]
        public async Task<ActionResult> UploadAdditionalImages(int id, [FromForm] List<IFormFile> images, [FromForm] List<int?> imageSortOrders, [FromForm] List<string> imageDescriptions)
        {
            if (images == null || images.Count == 0)
            {
                return BadRequest("No images provided.");
            }

            // Check if the InventoryItem exists
            if (!_context.InventoryItems.Any(item => item.Id == id))
            {
                return NotFound("Inventory item not found.");
            }

            // Clear existing images
            var existingImages = _context.AdditionalInvImages.Where(img => img.ItemId == id).ToList();
            if (existingImages.Any())
            {
                _context.AdditionalInvImages.RemoveRange(existingImages);
                _context.SaveChanges();
            }

            var additionalImages = new List<AdditionalInvImage>();

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                if (image.ContentType != "image/jpeg" && image.ContentType != "image/jpg" && image.ContentType != "image/png")
                {
                    return BadRequest("Invalid image format. Only JPEG and PNG are supported.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    additionalImages.Add(new AdditionalInvImage
                    {
                        ItemId = id,
                        ImageData = Convert.ToBase64String(memoryStream.ToArray()),
                        ImageSortOrder = imageSortOrders != null && imageSortOrders.Count > i ? imageSortOrders[i] : (int?)null,
                        ImageDescription = imageDescriptions != null && imageDescriptions.Count > i ? imageDescriptions[i] : null
                    });
                }
            }

            _context.AdditionalInvImages.AddRange(additionalImages);
            _context.SaveChanges();

            return Ok("Additional images uploaded and data saved successfully.");
        }

        // GET: api/Inventory/GetAdditionalImages/5
        [HttpGet("GetAdditionalImages/{id}")]
        public ActionResult<IEnumerable<object>> GetAdditionalImages(int id)
        {
            var additionalImages = _context.AdditionalInvImages
                                           .Where(img => img.ItemId == id)
                                           .OrderBy(img => img.ImageSortOrder ?? int.MaxValue) // Provide a default value if ImageSortOrder is null
                                           .Select(img => new
                                           {
                                               ImageData = img.ImageData ?? string.Empty, // Provide a default value if ImageData is null
                                               ImageDescription = img.ImageDescription ?? string.Empty // Provide a default value if ImageDescription is null
                                           })
                                           .ToList();

            if (additionalImages == null || !additionalImages.Any())
            {
                return NotFound("No additional images found for the inventory item.");
            }

            return Ok(additionalImages);
        }

        [HttpPost("UploadImage/{id}")]
        public async Task<ActionResult> UploadImage(int id, IFormFile image)
        {
            if (image == null || (image.ContentType != "image/jpeg" && image.ContentType != "image/jpg" && image.ContentType != "image/png"))
            {
                return BadRequest("Invalid image format. Only JPEG and PNG are supported.");
            }

            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Delete the existing image if it exists
            if (!string.IsNullOrEmpty(item.ImageFilePath))
            {
                var existingBlobClient = new BlobClient(new Uri(item.ImageFilePath));
                await existingBlobClient.DeleteIfExistsAsync();
            }

            var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(image.FileName));
            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            item.ImageFilePath = blobClient.Uri.ToString();
            _context.InventoryItems.Update(item);
            _context.SaveChanges();

            return Ok("Image uploaded and URL updated successfully.");
        }

        // GET: api/Inventory/Get/5
        [HttpGet("Get/{id}")]
        public ActionResult<InventoryItem> Get(int id)
        {
            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }
            return Ok(item);
        }

        // POST: api/Inventory/Create
        [HttpPost("Create")]
        public ActionResult Create([FromBody] InventoryItem item)
        {
            if (ModelState.IsValid)
            {
                _context.InventoryItems.Add(item);
                _context.SaveChanges();
                return Ok("Inventory item created successfully. ID: " + item.Id);
            }

            return BadRequest("Invalid data provided.");
        }

        // DELETE: api/Inventory/Delete/5
        [HttpDelete("Delete/{id}")]
        public ActionResult Delete(int id)
        {
            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }

            _context.InventoryItems.Remove(item);
            _context.SaveChanges();
            return Ok("Inventory item deleted successfully.");
        }

        // GET: api/Inventory/ListAll
        [HttpGet("ListAll")]
        public ActionResult<IEnumerable<InventoryItem>> ListAll(string searchText = null)
        {
            var items = _context.InventoryItems.ToList();

            if (!string.IsNullOrEmpty(searchText))
            {
                items = items.Where(item => item.Name.Contains(searchText) || item.Description.Contains(searchText)).ToList();
            }

            return Ok(items);
        }

        // GET: api/Inventory/Dummy
        [HttpGet("Dummy")]
        public ActionResult<IEnumerable<InventoryItem>> Dummy()
        {
            var items = new List<InventoryItem>();
            items.Add(new InventoryItem { Id = 29,Description="dummy inventory idtem;" });
            return Ok(items);
        }

        // PUT: api/Inventory/Edit/5
        [HttpPut("Edit/{id}")]
        public ActionResult Edit(int id, [FromBody] InventoryItem updatedItem)
        {
            if (id != updatedItem.Id)
            {
                return BadRequest("ID mismatch.");
            }

            var item = _context.InventoryItems.Find(id);
            if (item == null)
            {
                return NotFound("Inventory item not found.");
            }

            item.Name = updatedItem.Name;
            item.Description = updatedItem.Description;
            item.Unit = updatedItem.Unit;
            item.MinUnit = updatedItem.MinUnit;
            item.PricePerUnit = updatedItem.PricePerUnit;
            if (!string.IsNullOrEmpty(updatedItem.ImageFilePath))
            {
                item.ImageFilePath = updatedItem.ImageFilePath;
            }
            _context.InventoryItems.Update(item);
            _context.SaveChanges();
            return Ok("Inventory item updated successfully.");
        }

        // DELETE: api/Inventory/DeleteAll
        [HttpDelete("DeleteAll")]
        public ActionResult DeleteAll()
        {
            var items = _context.InventoryItems.ToList();
            if (!items.Any())
            {
                return NotFound("No inventory items found.");
            }

            _context.InventoryItems.RemoveRange(items);
            _context.SaveChanges();
            return Ok("All inventory items deleted successfully.");
        }
    }
}
