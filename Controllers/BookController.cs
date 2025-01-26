using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Readify.Data;
using Readify.Models;
using System.Security.Claims;

namespace Readify.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> MoreInfo(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.UploadedByUser)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpGet]
        public IActionResult AllBooks()
        {
            var books = _context.Books
                .Include(b => b.UploadedByUser)
                .Include(b => b.Comments)
                    .ThenInclude(c => c.User).ToList();

            return View(books);
        }

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            var books = await _context.Books
                .Where(b => b.Title.Contains(query) || b.Author.Contains(query))
                .ToListAsync();

            return View(books);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return AccessDenied();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book, IFormFile photoFile)
        {
            if (!IsAdmin()) return AccessDenied();

            ModelState.Clear();

            book.UploadedAt = DateTime.Now;
            book.UserId = 3;

            if (photoFile != null && photoFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(photoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Photo", "Допустимы только файлы изображений (JPG, JPEG, PNG, GIF).");
                    return View(book);
                }

                var fileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine("wwwroot/imgs", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                book.Photo = "imgs/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        var errorMessage = state.Errors.First().ErrorMessage;
                        Console.WriteLine($"Ошибка в поле {key}: {errorMessage}");
                    }
                }

                return View(book);
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllBooks));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return AccessDenied();

            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? newPhoto)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (newPhoto != null && newPhoto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(newPhoto.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await newPhoto.CopyToAsync(fileStream);
                }

                book.Photo = Path.Combine("imgs", uniqueFileName);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.BookId == id);
                    if (existingBook != null)
                    {
                        book.UserId = existingBook.UserId;
                    }

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.BookId == book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AllBooks));
            }

            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            return View(book);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin()) return AccessDenied();

            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Book book)
        {
            if (!IsAdmin()) return AccessDenied();

            if (book != null) _context.Books.Remove(book);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllBooks));
        }


        private bool BookExists(int id) => _context.Books.Any(e => e.BookId == id);
        private bool IsAdmin() => HttpContext.Session.GetString("UserId") == "3";
        private IActionResult AccessDenied()
        {
            TempData["Error"] = "Доступ запрещен!";
            return RedirectToAction("AllBooks");
        }
    }
}
