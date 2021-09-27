using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;

namespace AuthSystem.Controllers
{
    // Show views only if user is logged in (authenticated).
    [Authorize]
    public class GameController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public GameController(AuthDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }

        // GET: MyGames
        public async Task<IActionResult> MyGames(string sortOrder, string searchString)
        {
            var games = from g in _context.Games
                        .Include(g => g.ApplicationUser)
                        select g;

            if (!String.IsNullOrEmpty(searchString))
            {
                games = games.Where(s =>
                    s.Genre.Contains(searchString) ||
                    s.Name.Contains(searchString) ||
                    s.ApplicationUser.FirstName.Contains(searchString)
                );
            }

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["GenreSortParm"] = sortOrder == "Genre" ? "genre_desc" : "Genre";
   
            switch (sortOrder)
            {
                case "name_desc":
                    games = games.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    games = games.OrderBy(s => s.Date);
                    break;
                case "date_desc":
                    games = games.OrderByDescending(s => s.Date);
                    break;
                case "Genre":
                    games = games.OrderBy(s => s.Genre);
                    break;
                case "genre_desc":
                    games = games.OrderByDescending(s => s.Genre);
                    break;
                default:
                    games = games.OrderBy(s => s.Name);
                    break;
            }
            return View(await games.AsNoTracking().ToListAsync());
        }

        // GET: Index
        [Route("")]
        [Route("Game")]
        [Route("Game/Index")]
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {           
            var games = from m in _context.Games
                          select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                games = games.Where(s => 
                    s.Genre.Contains(searchString) ||
                    s.Name.Contains(searchString) ||
                    s.ApplicationUser.FirstName.Contains(searchString)
                );
            }

            ViewData["DateSortParm"] = sortOrder == "date_desc" ? "Date" : "date_desc";

            switch (sortOrder)
            {
                case "date_desc":
                    games = games.OrderByDescending(s => s.Date);
                    break;
                case "Date":
                    games = games.OrderBy(s => s.Date);
                    break;
                default:
                    games = games.OrderBy(s => s.Date);
                    break;
            }
            return View(await games.AsNoTracking().ToListAsync());
        }

        // GET: Game/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Game/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Game/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Genre,Date,ImageFile")] Game game)
        {
            if (ModelState.IsValid)
            {
                // Saves images to wwroot/img folder.
                string wwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(game.ImageFile.FileName);
                string extension = Path.GetExtension(game.ImageFile.FileName);
                game.ImageName=fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwRootPath + "/img/userGameImg/", fileName);
                using (var filestream = new FileStream(path, FileMode.Create))
                {
                    await game.ImageFile.CopyToAsync(filestream);
                }

                _context.Add(game);

                var todaysDate = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
                game.Date = todaysDate;

                var currentUser = await _userManager.GetUserAsync(User);
                game.UserFK = currentUser.Id;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyGames));
            }
            return View(game);
        }

        // GET: Game/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
            .Include(g => g.ApplicationUser)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null)
            {
                return NotFound();
            }
            return View(game);
        }

        // POST: Game/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Genre,Date,ImageFile")] Game game)
        {
            if (id != game.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // DOESN'T REPLACE THE OLD IMAGE, JUST CREATES A NEW TO THE FOLDER.
                    // Saves images to wwroot/img folder.
                    string wwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(game.ImageFile.FileName);
                    string extension = Path.GetExtension(game.ImageFile.FileName);
                    game.ImageName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwRootPath + "/img/userGameImg/", fileName);            
                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await game.ImageFile.CopyToAsync(filestream);
                    }
                                       
                    var todaysDate = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
                    game.Date = todaysDate; 

                    var currentUser = await _userManager.GetUserAsync(User);
                    game.UserFK = currentUser.Id; 

                    _context.Update(game);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.Id))
                    {
                        return NotFound();
                    }
                    else
                    {                
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyGames));
            }
            return View(game);
        }

        // GET: Game/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // POST: Game/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);

            // Delete image from wwroot/img folder.
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "img/userGameImg/", game.ImageName);
            if (System.IO.File.Exists(imagePath))         
                System.IO.File.Delete(imagePath);

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }

        // GET: Game/About
        public IActionResult About()
        {
            Employee[] employees = new Employee[]
            {
                new Employee { Name="Evelyn",WorkTitle="CEO",ImageName="ceo.jpg" },
                new Employee { Name="Chloe",WorkTitle="Lead Programmer",ImageName="leadprog.jpg" },
                new Employee { Name="Mark",WorkTitle="Programmer",ImageName="prog.jpg" }
            };
            return View(employees);
        }
    }
}
