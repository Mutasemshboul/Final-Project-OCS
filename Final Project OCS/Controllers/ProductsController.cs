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
    public class ProductsController : BaseController
    {


        public ProductsController(ApplicationDbContext context , ChatService chatService, UserManager<IdentityUser> userManager) :base(chatService, context, userManager)
        {
            
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");
            IQueryable<Product> applicationDbContext;

            if (isAdmin)
            {
                applicationDbContext = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(p=>!p.IsDeleted && !p.Category.IsDeleted);
            }
            else
            {
                applicationDbContext = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.User)
                    .Where(u => u.UserId == userId && !u.IsDeleted && !u.Category.IsDeleted);
            }
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => !c.IsDeleted), "Id", "CategoryName");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "UserName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create( Product product)
        {
            bool isAdmin = User.IsInRole("Admin");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!isAdmin)
            {
                
                product.UserId = userId;
                
            }
            

            bool isRichMaxNumOfProduct = this.CheckNumberOfProduct();

            if (isRichMaxNumOfProduct)
            {
                return Json(new { success = false, message = "You have reached the maximum number of products allowed by your subscription." });
            }

            if (ModelState.IsValid)
            {
                var user = await _context.ApplicationUsers.FindAsync(userId);
                user.NumberOfAds += 1;
                _context.Add(product);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Product created successfully." });
            }

            return Json(new { success = false, message = "There was an error with your submission." });
        }


        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var userProducs = await _context.Products.FirstOrDefaultAsync(s => s.UserId == userId && s.Id == id);

            if (userProducs == null || id != userProducs.Id)
            {
                return Unauthorized("You do not have access to edit this product.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoryName", product.CategoryId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "UserName", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Price,Id,UserId,CategoryId,Title,Description,Status,IsDeleted")] Product product)
        {
            if (id != product.Id)
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
                        product.UserId = userId;
                    }
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                //_context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
