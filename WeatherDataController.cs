using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class WeatherDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WeatherDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WeatherData - Доступно всем (включая неавторизованным)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var weatherData = await _context.WeatherDataSet
                .Include(w => w.City)
                .OrderByDescending(w => w.Date)
                .ToListAsync();

            return View(weatherData);
        }

        // GET: WeatherData/Details/5 - Доступно всем
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var weatherData = await _context.WeatherDataSet
                .Include(w => w.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (weatherData == null)
                return NotFound();

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            };
            HttpContext.Response.Cookies.Append("LastViewedWeatherId", id?.ToString() ?? "0", cookieOptions);

            return View(weatherData);
        }

        // GET: WeatherData/Create - Только Manager и Admin
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Create()
        {
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            return View();
        }

        // POST: WeatherData/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Create([Bind("CityId,Temperature,Humidity,WindSpeed,Description")] WeatherData weatherData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(weatherData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Данные о погоде успешно добавлены";
                return RedirectToAction(nameof(Index));
            }
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", weatherData.CityId);
            return View(weatherData);
        }

        // GET: WeatherData/Edit/5
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var weatherData = await _context.WeatherDataSet.FindAsync(id);
            if (weatherData == null)
                return NotFound();

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", weatherData.CityId);
            return View(weatherData);
        }

        // POST: WeatherData/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CityId,Temperature,Humidity,WindSpeed,Description")] WeatherData weatherData)
        {
            if (id != weatherData.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weatherData);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Данные о погоде успешно обновлены";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeatherDataExists(weatherData.Id))
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
            if (id == null)
                return NotFound();

            var weatherData = await _context.WeatherDataSet
                .Include(w => w.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (weatherData == null)
                return NotFound();

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
                TempData["Success"] = "Данные о погоде успешно удалены";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool WeatherDataExists(int id)
        {
            return _context.WeatherDataSet.Any(e => e.Id == id);
        }
    }
}