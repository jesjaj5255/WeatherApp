using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Data;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class CitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cities
        public async Task<IActionResult> Index()
        {
            var cities = await _context.Cities.ToListAsync();
            return View(cities);
        }

        // GET: Cities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var city = await _context.Cities
                .Include(c => c.WeatherHistory.OrderByDescending(w => w.Date))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (city == null)
                return NotFound();

            // Query String - график
            ViewBag.ShowChart = Request.Query["chart"] == "true";

            return View(city);
        }

        // GET: Cities/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cities/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Country,Latitude,Longitude,Description")] City city)
        {
            if (ModelState.IsValid)
            {
                _context.Add(city);
                await _context.SaveChangesAsync();

                // Инвалидация кэша
                var cache = HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
                cache?.Remove("cities_list");

                TempData["SuccessMessage"] = $"Город '{city.Name}' успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        // GET: Cities/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound();

            return View(city);
        }

        // POST: Cities/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Country,Latitude,Longitude,Description,CreatedDate")] City city)
        {
            if (id != city.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(city);
                    await _context.SaveChangesAsync();

                    // Инвалидация кэша
                    var cache = HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
                    cache?.Remove("cities_list");

                    TempData["SuccessMessage"] = "Город успешно обновлён!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(city.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(city);
        }

        // GET: Cities/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var city = await _context.Cities.FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
                return NotFound();

            return View(city);
        }

        // POST: Cities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();

                // Инвалидация кэша
                var cache = HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
                cache?.Remove("cities_list");

                TempData["SuccessMessage"] = "Город успешно удалён!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }
    }
}