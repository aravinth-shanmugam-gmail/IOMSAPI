using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace IOMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IOMSContext _context;

        public InventoryController(IOMSContext context)
        {
            _context = context;
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
