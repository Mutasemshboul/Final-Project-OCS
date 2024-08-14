using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using System.Security.Claims;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Identity;

namespace Final_Project_OCS.Controllers
{
    public class ProductSwapsController : BaseController
    {

        public ProductSwapsController(ApplicationDbContext context , ChatService chatService, UserManager<IdentityUser> userManager) :base(chatService, context, userManager)
        {
            
        }

        // GET: ProductSwaps
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            IQueryable<ProductSwap> applicationDbContext;

            if (isAdmin)
            {
                applicationDbContext = _context.ProductSwaps
                    .Include(p => p.Category)
                    .Include(p => p.User);
            }
            else
            {
                applicationDbContext = _context.ProductSwaps
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(u => u.UserId == userId);
            }
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProductSwaps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productSwap = await _context.ProductSwaps
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productSwap == null)
            {
                return NotFound();
            }

            return View(productSwap);
        }

        // GET: ProductSwaps/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "UserName");
            return View();
        }

        // POST: ProductSwaps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SwapDate,Id,UserId,CategoryId,Title,Description,Status,IsDeleted")] ProductSwap productSwap)
        {
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                productSwap.UserId = userId;
            }
            if (ModelState.IsValid)
            {
                _context.Add(productSwap);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", productSwap.CategoryId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "UserName", productSwap.UserId);
            return View(productSwap);
        }

        // GET: ProductSwaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productSwap = await _context.ProductSwaps.FindAsync(id);
            if (productSwap == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", productSwap.CategoryId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "UserName", productSwap.UserId);
            return View(productSwap);
        }

        // POST: ProductSwaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SwapDate,Id,UserId,CategoryId,Title,Description,Status,IsDeleted")] ProductSwap productSwap)
        {
            if (id != productSwap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool isAdmin = User.IsInRole("Admin");
                    if (!isAdmin)
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        productSwap.UserId = userId;
                    }
                    _context.Update(productSwap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductSwapExists(productSwap.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", productSwap.CategoryId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", productSwap.UserId);
            return View(productSwap);
        }

        // GET: ProductSwaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productSwap = await _context.ProductSwaps
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productSwap == null)
            {
                return NotFound();
            }

            return View(productSwap);
        }

        // POST: ProductSwaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productSwap = await _context.ProductSwaps.FindAsync(id);
            if (productSwap != null)
            {
                _context.ProductSwaps.Remove(productSwap);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductSwapExists(int id)
        {
            return _context.ProductSwaps.Any(e => e.Id == id);
        }
    }
}
