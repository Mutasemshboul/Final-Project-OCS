using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Final_Project_OCS.Controllers
{
    public class StoreProductsController : BaseController
    {

        public StoreProductsController(ApplicationDbContext context, ChatService chatService, UserManager<IdentityUser> userManager) : base(chatService, context, userManager)
        {

        }

        // GET: StoreProducts
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); 
            }

            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null)
            {
                return NotFound("User does not have a store.");
            }

            var storeProducts = _context.StoreProducts
                                        .Include(s => s.Category)
                                        .Include(s => s.Store)
                                        .Where(s => s.StoreId == userStore.Id);

            return View(await storeProducts.ToListAsync());
        }


        // GET: StoreProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeProduct = await _context.StoreProducts
                .Include(s => s.Category)
                .Include(s => s.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storeProduct == null)
            {
                return NotFound();
            }

            return View(storeProduct);
        }

        // GET: StoreProducts/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); 
            }

            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null)
            {
                return NotFound("User does not have a store.");
            }

            ViewData["CategoryId"] = new SelectList(_context.StoreCategories.Where(c => c.StoreId == userStore.Id), "Id", "Name");

            return View();
        }


        // POST: StoreProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoryId,Title,Description,Price,Status,IsDeleted,Code")] StoreProduct storeProduct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); 
            }

            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null)
            {
                return NotFound("User does not have a store.");
            }

            storeProduct.StoreId = userStore.Id;

            if (ModelState.IsValid)
            {
                _context.Add(storeProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.StoreCategories.Where(c => c.StoreId == userStore.Id), "Id", "Name", storeProduct.CategoryId);

            return View(storeProduct);
        }


        // GET: StoreProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeProduct = await _context.StoreProducts
                .Include(sp => sp.Store)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (storeProduct == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); 
            }

            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null || storeProduct.StoreId != userStore.Id)
            {
                return Unauthorized("You do not have access to edit this product."); 
            }

            ViewData["CategoryId"] = new SelectList(_context.StoreCategories.Where(c => c.StoreId == userStore.Id), "Id", "Name", storeProduct.CategoryId);

            return View(storeProduct);
        }


        // POST: StoreProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryId,Title,Description,Price,Status,IsDeleted,Code")] StoreProduct storeProduct)
        {
            if (id != storeProduct.Id)
            {
                return NotFound();
            }

            // Get the currently logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); // If the user is not logged in
            }

            // Find the store associated with the current user
            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null)
            {
                return NotFound("User does not have a store.");
            }

            // Automatically assign the StoreId to ensure it's correct
            storeProduct.StoreId = userStore.Id;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(storeProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoreProductExists(storeProduct.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Load the categories associated with the user's store for the dropdown
            ViewData["CategoryId"] = new SelectList(_context.StoreCategories.Where(c => c.StoreId == userStore.Id), "Id", "Name", storeProduct.CategoryId);

            return View(storeProduct);
        }


        // GET: StoreProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeProduct = await _context.StoreProducts
                .Include(s => s.Category)
                .Include(s => s.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storeProduct == null)
            {
                return NotFound();
            }

            return View(storeProduct);
        }

        // POST: StoreProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var storeProduct = await _context.StoreProducts.FindAsync(id);
            if (storeProduct != null)
            {
                _context.StoreProducts.Remove(storeProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoreProductExists(int id)
        {
            return _context.StoreProducts.Any(e => e.Id == id);
        }
    }
}
