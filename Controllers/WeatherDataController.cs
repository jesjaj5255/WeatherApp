using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers
{
    public class WeatherDataController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WeatherVisitCounter _visitCounter;

        public WeatherDataController(ApplicationDbContext context, WeatherVisitCounter visitCounter)
        {
            _context = context;
            _visitCounter = visitCounter;
        }

        // GET: WeatherData - accessible by everyone
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? search, int? cityId, string? date, string? viewMode)
        {
            // APPLICATION-LEVEL STATE: global visit counter
            var globalVisits = _visitCounter.Increment();

            // SESSION: remember last search text
            if (!string.IsNullOrEmpty(search))
                HttpContext.Session.SetString("LastSearch", search);
            else
                search = HttpContext.Session.GetString("LastSearch");

            // COOKIE: remember preferred view mode (table/cards)
            if (!string.IsNullOrEmpty(viewMode))
            {
                Response.Cookies.Append("ViewMode", viewMode,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), Secure = true, SameSite = SameSiteMode.Lax });
            }
            else
            {
                viewMode = Request.Cookies["ViewMode"] ?? "table";
            }

            var query = _context.WeatherDataSet.Include(w => w.City).AsQueryable();

            // QUERY STRING: filter by city
            if (cityId.HasValue)
                query = query.Where(w => w.CityId == cityId.Value);

            // QUERY STRING: filter by description/search
            if (!string.IsNullOrEmpty(search))
                query = query.Where(w => w.Description.Contains(search) || (w.City != null && w.City.Name.Contains(search)));

            // QUERY STRING: filter by date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                query = query.Where(w => w.Date.Date == parsedDate.Date);

            var weatherData = await query.OrderByDescending(w => w.Date).ToListAsync();

            ViewBag.GlobalVisits = globalVisits;
            ViewBag.ViewMode = viewMode;
            ViewBag.Search = search ?? "";
            ViewBag.CityId = cityId;
            ViewBag.Date = date ?? "";
            ViewBag.Cities = new SelectList(await _context.Cities.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", cityId);

            return View(weatherData);
        }

        // GET: WeatherData/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var weatherData = await _context.WeatherDataSet
                .Include(w => w.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (weatherData == null) return NotFound();

            Response.Cookies.Append("LastViewedWeatherId", id.ToString()!,
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), Secure = true, SameSite = SameSiteMode.Lax });

            return View(weatherData);
        }

        // GET: WeatherData/Create - Admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            return View();
        }

        // POST: WeatherData/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("CityId,Temperature,Humidity,WindSpeed,Description,Date")] WeatherData weatherData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(weatherData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Weather record added successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", weatherData.CityId);
            return View(weatherData);
        }

        // GET: WeatherData/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var weatherData = await _context.WeatherDataSet.FindAsync(id);
            if (weatherData == null) return NotFound();

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", weatherData.CityId);
            return View(weatherData);
        }

        // POST: WeatherData/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CityId,Temperature,Humidity,WindSpeed,Description,Date")] WeatherData weatherData)
        {
            if (id != weatherData.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weatherData);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Weather record updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.WeatherDataSet.Any(e => e.Id == weatherData.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", weatherData.CityId);
            return View(weatherData);
        }

        // GET: WeatherData/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var weatherData = await _context.WeatherDataSet
                .Include(w => w.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (weatherData == null) return NotFound();
            return View(weatherData);
        }

        // POST: WeatherData/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var weatherData = await _context.WeatherDataSet.FindAsync(id);
            if (weatherData != null)
            {
                _context.WeatherDataSet.Remove(weatherData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Weather record deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
