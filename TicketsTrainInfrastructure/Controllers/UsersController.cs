using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TicketsTrainDomain.Model;
using TicketsTrainInfrastructure;

namespace TicketsTrainInfrastructure.Controllers
{
    public class UsersController : Controller
    {
        private readonly TicketsTrainContext _context;

        public UsersController(TicketsTrainContext context)
        {
            _context = context;
        }

        // GET: Користувачі
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Список користувачів";
            return View(await _context.Users.ToListAsync());
        }

        // GET: Користувачі/Деталі/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Деталі користувача";
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Користувачі/Створити
        public IActionResult Create()
        {
            ViewData["Title"] = "Створення користувача";
            return View();
        }

        // POST: Користувачі/Створити
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,PhoneNumber,Surname,Email")] User user) // Прибрали Id з Bind
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user); 
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Створення користувача";
            return View(user);
        }

        // GET: Користувачі/Редагувати/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Редагування користувача";
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Користувачі/Редагувати/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PhoneNumber,Surname,Email")] User user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Користувачі/Видалити/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            ViewData["Title"] = "Видалення користувача";
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Користувачі/Видалити/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
