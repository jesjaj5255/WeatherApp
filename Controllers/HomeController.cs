using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private const string CITIES_CACHE_KEY = "cities_list";
        private const int CACHE_DURATION_MINUTES = 10;

        public HomeController(ApplicationDbContext context, IMemoryCache cache, UserManager<User> userManager)
        {
            _context = context;
            _cache = cache;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string sortBy = "name", string? country = null, int page = 1)
        {
            // SESSION: track visit count
            var visitCount = HttpContext.Session.GetInt32("VisitCount") ?? 0;
            HttpContext.Session.SetInt32("VisitCount", visitCount + 1);
            if (HttpContext.Session.GetString("FirstVisit") == null)
                HttpContext.Session.SetString("FirstVisit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var firstVisit = HttpContext.Session.GetString("FirstVisit") ?? "Unknown";

            // COOKIE: remember last visit time
            var lastView = Request.Cookies["LastVisit"];
            Response.Cookies.Append("LastVisit", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax });

            // CACHE: cache city list for 10 minutes
            if (!_cache.TryGetValue(CITIES_CACHE_KEY, out List<City>? cachedCities))
            {
                cachedCities = await _context.Cities.ToListAsync();
                _cache.Set(CITIES_CACHE_KEY, cachedCities, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));
            }

            var citiesList = cachedCities ?? new List<City>();

            // QUERY STRING: filter by country
            if (!string.IsNullOrEmpty(country))
                citiesList = citiesList.Where(c => c.Country.Contains(country, StringComparison.OrdinalIgnoreCase)).ToList();

            // QUERY STRING: sort
            citiesList = sortBy switch
            {
                "country" => citiesList.OrderBy(c => c.Country).ToList(),
                "name_desc" => citiesList.OrderByDescending(c => c.Name).ToList(),
                _ => citiesList.OrderBy(c => c.Name).ToList()
            };

            // QUERY STRING: pagination
            const int pageSize = 6;
            var totalCount = citiesList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            citiesList = citiesList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.VisitCount = visitCount + 1;
            ViewBag.FirstVisit = firstVisit;
            ViewBag.LastVisit = lastView ?? "First visit";
            ViewBag.SortBy = sortBy;
            ViewBag.FilterCountry = country ?? "";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCities = totalCount;

            return View(citiesList);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
