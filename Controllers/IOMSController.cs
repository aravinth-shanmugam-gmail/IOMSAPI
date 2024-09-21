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

        // POST: api/Inventory/UploadImage/5
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
        public ActionResult<IEnumerable<InventoryItem>> ListAll()
        {
            var items = _context.InventoryItems.ToList();
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
