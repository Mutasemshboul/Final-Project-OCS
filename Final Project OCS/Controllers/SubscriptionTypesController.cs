using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Authorization;

namespace Final_Project_OCS.Controllers
{
    [Authorize(Roles =SD.SD.Role_Admin)]
    public class SubscriptionTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubscriptionTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.SubscriptionTypes.Where(su=>!su.IsDeleted).ToListAsync());
        }

        // GET: SubscriptionTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionType = await _context.SubscriptionTypes
                .FirstOrDefaultAsync(m => m.SubscriptionTypeId == id);
            if (subscriptionType == null)
            {
                return NotFound();
            }

            return View(subscriptionType);
        }

        // GET: SubscriptionTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SubscriptionTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SubscriptionTypeId,Name,Description,Price,NumberOfAdsAllowed,IsDeleted")] SubscriptionType subscriptionType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subscriptionType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(subscriptionType);
        }

        // GET: SubscriptionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionType = await _context.SubscriptionTypes.FindAsync(id);
            if (subscriptionType == null)
            {
                return NotFound();
            }
            return View(subscriptionType);
        }

        // POST: SubscriptionTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SubscriptionTypeId,Name,Description,Price,NumberOfAdsAllowed,IsDeleted")] SubscriptionType subscriptionType)
        {
            if (id != subscriptionType.SubscriptionTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subscriptionType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionTypeExists(subscriptionType.SubscriptionTypeId))
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
            return View(subscriptionType);
        }

        // GET: SubscriptionTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subscriptionType = await _context.SubscriptionTypes
                .FirstOrDefaultAsync(m => m.SubscriptionTypeId == id);
            if (subscriptionType == null)
            {
                return NotFound();
            }

            return View(subscriptionType);
        }

        // POST: SubscriptionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscriptionType = await _context.SubscriptionTypes.FindAsync(id);
            if (subscriptionType != null)
            {
                _context.SubscriptionTypes.Remove(subscriptionType);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionTypeExists(int id)
        {
            return _context.SubscriptionTypes.Any(e => e.SubscriptionTypeId == id);
        }
    }
}
