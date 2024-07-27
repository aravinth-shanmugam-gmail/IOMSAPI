using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IOMSAPI.Controllers
{

    public class IOMSController : Controller
    {
        private readonly IOMSContext _context;

        public IOMSController(IOMSContext context)
        {
            _context = context;
        }

        // GET: IOMSController
        public ActionResult Index()
        {
            return View();
        }

        // GET: IOMSController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: IOMSController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IOMSController/Create
        [Route("api/[controller]/CreateInventoryItem")]
        [HttpPost]
        public ActionResult CreateInventoryItem(InventoryItem item)
        {
            if (ModelState.IsValid)
            {
                _context.InventoryItems.Add(item);
                _context.SaveChanges();
                return Ok("Inventory item created successfully." + item.Id);
            }

            return BadRequest("Invalid data provided.");
        }

        // GET: IOMSController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: IOMSController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: IOMSController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: IOMSController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
