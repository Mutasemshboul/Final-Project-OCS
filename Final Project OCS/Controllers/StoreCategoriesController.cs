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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Final_Project_OCS.Controllers
{
    

    public class StoreCategoriesController : BaseController
    {

        public StoreCategoriesController(ApplicationDbContext context, ChatService chatService, UserManager<IdentityUser> userManager) : base(chatService, context, userManager)
        {

        }

        // GET: StoreCategories
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

            var storeCategories = _context.StoreCategories
                                          .Include(s => s.Store)
                                          .Where(s => s.StoreId == userStore.Id && !s.IsDeleted);

            return View(await storeCategories.ToListAsync());
        }


        // GET: StoreCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeCategory = await _context.StoreCategories
                .Include(s => s.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storeCategory == null)
            {
                return NotFound();
            }

            return View(storeCategory);
        }

        // GET: StoreCategories/Create
        public IActionResult Create()
        {
            ViewData["StoreId"] = new SelectList(_context.Stores, "Id", "Id");
            return View();
        }

        // POST: StoreCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] StoreCategory storeCategory)
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

            
            storeCategory.StoreId = userStore.Id;
            if (ModelState.IsValid)
            {
                _context.Add(storeCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoreId"] = new SelectList(_context.Stores, "Id", "Id", storeCategory.StoreId);
            return View(storeCategory);
        }

        // GET: StoreCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeCategory = await _context.StoreCategories.FindAsync(id);
            if (storeCategory == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized(); 
            }

            var userStore = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);

            if (userStore == null || storeCategory.StoreId != userStore.Id)
            {
                return Unauthorized("You do not have access to edit this category."); 
            }

            return View(storeCategory);
        }


        // POST: StoreCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] StoreCategory storeCategory)
        {
            if (id != storeCategory.Id)
            {
                return NotFound();
            }

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

            var existingStoreCategory = await _context.StoreCategories
                .FirstOrDefaultAsync(sc => sc.Id == id && sc.StoreId == userStore.Id);

            if (existingStoreCategory == null)
            {
                return Unauthorized("You do not have access to edit this category.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingStoreCategory.Name = storeCategory.Name;
                    existingStoreCategory.Description = storeCategory.Description;

                    _context.Update(existingStoreCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoreCategoryExists(storeCategory.Id))
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

            return View(storeCategory);
        }


        // GET: StoreCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storeCategory = await _context.StoreCategories
                .Include(s => s.Store)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (storeCategory == null)
            {
                return NotFound();
            }

            return View(storeCategory);
        }

        // POST: StoreCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var storeCategory = await _context.StoreCategories.FindAsync(id);
            if (storeCategory != null)
            {
                _context.StoreCategories.Remove(storeCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoreCategoryExists(int id)
        {
            return _context.StoreCategories.Any(e => e.Id == id);
        }
    }
}
