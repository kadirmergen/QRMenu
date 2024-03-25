using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenu.Data;
using QRMenu.Models;

namespace QRMenu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        
        public CategoriesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
          if (_context.Categories == null)
          {
              return NotFound();
          }
            return await _context.Categories.ToListAsync();
            
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdministrator, RestaurantAdministrator")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if(User.HasClaim("CompanyId",id.ToString()) || User.HasClaim("RestaurantId",id.ToString()))
            {
                if (id != category.Id)
                {
                    return BadRequest();
                }
                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return Ok("Category updated!");
        }

        // POST: api/Categories
        [HttpPost]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            
            _context.Categories!.Add(category);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("UploadImage/{categoryId}")]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<IActionResult> UploadImage(int categoryId, IFormFile file)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category cannot found");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest();
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest();
            }

            var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine("wwwroot", "images");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            category.Image = $"/images/{fileName}";
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok("Image uploaded.");
        }

        [HttpPut("EditImage/{categoryId}")]
        [Authorize(Roles = "CompanyAdministrator")] 
        public async Task<IActionResult> EditImage(int categoryId, IFormFile file)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category cannot found.");
            }

            if (!string.IsNullOrEmpty(category.Image))
            {
                var imagePath = Path.Combine("wwwroot", category.Image.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest();
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest();
            }

            var fileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine("wwwroot", "images");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            category.Image = $"/images/{fileName}";
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return Ok("Image added.");
        }


        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdministrator, RestaurantAdministrator")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories!.FindAsync(id);
            category!.StateId = 0;
            _context.Categories.Update(category);

            var foods=_context.Foods.Where(f => f.CategoryId==category.Id);
            foreach (Food food in foods)
            {
                food.StateId = 0;
            }
            await _context.SaveChangesAsync();
            return Ok("Category has been deleted.");
        }
    }
}
