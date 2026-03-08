using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WeatherApp.Data;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CITIES_CACHE_KEY = "cities_list";
        private const int CACHE_DURATION_MINUTES = 60;

        public HomeController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string sortBy = "name", string? country = null, int page = 1)
        {
            // ========== СЕССИИ ==========
            if (HttpContext.Session.GetInt32("VisitCount") == null)
            {
                HttpContext.Session.SetInt32("VisitCount", 0);
                HttpContext.Session.SetString("FirstVisit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            var visitCount = HttpContext.Session.GetInt32("VisitCount") ?? 0;
            HttpContext.Session.SetInt32("VisitCount", visitCount + 1);
            var firstVisit = HttpContext.Session.GetString("FirstVisit") ?? "Неизвестно";

            // ========== КУКИ ==========
            var lastView = Request.Cookies["LastVisit"];
            Response.Cookies.Append("LastVisit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true
                });

            // ========== CACHE ==========
            if (!_cache.TryGetValue(CITIES_CACHE_KEY, out List<City>? cachedCities))
            {
                cachedCities = await _context.Cities.ToListAsync();
                _cache.Set(CITIES_CACHE_KEY, cachedCities, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            }

            var citiesList = cachedCities ?? new List<City>();

            // ========== ПРИМЕНЕНИЕ ФИЛЬТРА (Query String) ==========
            if (!string.IsNullOrEmpty(country))
            {
                citiesList = citiesList
                    .Where(c => c.Country.Contains(country, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // ========== СОРТИРОВКА (Query String) ==========
            citiesList = sortBy switch
            {
                "country" => citiesList.OrderBy(c => c.Country).ToList(),
                "name_desc" => citiesList.OrderByDescending(c => c.Name).ToList(),
                _ => citiesList.OrderBy(c => c.Name).ToList()
            };

            // ========== PAGINATION (Query String) ==========
            const int pageSize = 3;
            var totalCount = citiesList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            citiesList = citiesList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ========== VIEWBAG (для передачи данных в View) ==========
            ViewBag.VisitCount = visitCount + 1;
            ViewBag.FirstVisit = firstVisit;
            ViewBag.LastVisit = lastView ?? "Первый визит";
            ViewBag.SortBy = sortBy;
            ViewBag.FilterCountry = country ?? "";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCities = totalCount;

            return View(citiesList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}