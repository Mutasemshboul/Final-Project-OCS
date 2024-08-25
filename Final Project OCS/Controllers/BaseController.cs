using Final_Project_OCS.Data;
using Final_Project_OCS.Models;
using Final_Project_OCS.SD;
using Final_Project_OCS.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Final_Project_OCS.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        private readonly ChatService _chatService;
        protected readonly ApplicationDbContext _context;
        protected readonly UserManager<IdentityUser> _userManager;
        public static bool IsStoreProduct { get; set; }


        public BaseController(ChatService chatService , ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _chatService = chatService;
            _context = context;
            _userManager = userManager;

        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ViewBag.ChatUsers = await _chatService.GetChatUsersAsync(User);
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                ViewBag.HasStore = _context.Stores.Any(s => s.UserId == userId);
            }
            else
            {
                ViewBag.HasStore = false;
            }
            base.OnActionExecuting(context);

            await next();
        }

        public bool CheckNumberOfProduct()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return false;
            }

            var user = _context.ApplicationUsers
                .FirstOrDefault(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
            {
                return false;
            }

            

            return user.NumberOfAds >= user.NumberOfAdsAllowed;
        }
        [HttpPost]
        public async Task<IActionResult> GetImagesForProduct(int productId,bool isStoreProduct)
        {
            if (isStoreProduct)
            {
                var images = await _context.StoreProductImages
                                       .Where(i => i.ItemId == productId)
                                       .ToListAsync();
                ViewBag.ProductId = productId;
                IsStoreProduct = isStoreProduct;

                return PartialView("_ImageListPartial", images);
                
            }
            else
            {
                var images = await _context.ImageItems
                                       .Where(i => i.ItemId == productId)
                                       .ToListAsync();
                ViewBag.ProductId = productId;
                IsStoreProduct = isStoreProduct;


                return PartialView("_ImageListPartial", images);
            }
            
        }


        [HttpPost]
        public async Task<IActionResult> CreateImages(int itemId, IFormFile[] imageFiles)
        {
            if (imageFiles == null || imageFiles.Length == 0)
            {
                return BadRequest("No images selected.");
            }

            foreach (var imageFile in imageFiles)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetFileNameWithoutExtension(imageFile.FileName);
                var extension = Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine("wwwroot/images/products", fileName + extension);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                if (IsStoreProduct)
                {
                    var imageItem = new StoreProductImage
                    {
                        ImageUrl = "/images/products/" + fileName + extension,
                        ItemId = itemId
                    };
                    _context.StoreProductImages.Add(imageItem);
                }
                else
                {
                    var imageItem = new ImageItem
                    {
                        ImageUrl = "/images/products/" + fileName + extension,
                        ItemId = itemId
                    };
                    _context.ImageItems.Add(imageItem);
                }
                

                
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Image Added successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> EditImage(int imageId, IFormFile newImageFile)
        {
            if (newImageFile == null || imageId == 0)
            {
                return Json(new { success = false, message = "Invalid image or image ID." });
            }
            dynamic imageItem;
            if (IsStoreProduct)
            {
                imageItem = await _context.StoreProductImages.FindAsync(imageId);
            }
            else
            {
                imageItem = await _context.ImageItems.FindAsync(imageId);
            }
            if (imageItem == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }
            var currentImagePath = Path.Combine("wwwroot", imageItem.ImageUrl.TrimStart('/'));

            if (System.IO.File.Exists(currentImagePath))
            {
                System.IO.File.Delete(currentImagePath);
            }

            var fileName = Guid.NewGuid().ToString()+ Path.GetFileNameWithoutExtension(newImageFile.FileName);
            var extension = Path.GetExtension(newImageFile.FileName);
            var filePath = Path.Combine("wwwroot/images/products", fileName + extension);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await newImageFile.CopyToAsync(fileStream);
            }

            imageItem.ImageUrl = "/images/products/" + fileName + extension;

            if (IsStoreProduct)
            {
                _context.StoreProductImages.Update(imageItem);
            }
            else
            {
                _context.ImageItems.Update(imageItem);
            }

            
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Image updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            dynamic imageItem;
            if(IsStoreProduct)
            {
                imageItem = await _context.StoreProductImages.FindAsync(imageId);
            }
            else
            {
                imageItem = await _context.ImageItems.FindAsync(imageId);
            }

            if (imageItem == null)
            {
                return Json(new { success = false, message = "Image not found." });
            }
            if (IsStoreProduct)
            {
                _context.StoreProductImages.Remove(imageItem);
            }
            else
            {
                _context.ImageItems.Remove(imageItem);
            }
                

            await _context.SaveChangesAsync();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageItem.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return Json(new { success = true, message = "Image deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllImages(int itemId)
        {
            dynamic imageItems;
            if (IsStoreProduct)
            {
                imageItems = _context.StoreProductImages.Where(img => img.ItemId == itemId).ToList();
            }
            else
            {
                imageItems = _context.ImageItems.Where(img => img.ItemId == itemId).ToList();
            }

            if (imageItems == null || imageItems.Count == 0)
            {
                return Json(new { success = false, message = "No images found for this item." });
            }

            foreach (var imageItem in imageItems)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageItem.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                if (IsStoreProduct)
                {
                    _context.StoreProductImages.Remove(imageItem);
                }
                else
                {
                    _context.ImageItems.Remove(imageItem);
                }
                    
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "All images deleted successfully." });
        }



    }
}
