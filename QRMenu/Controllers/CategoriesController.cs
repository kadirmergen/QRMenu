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
            if (id != category.Id)
            {
                return BadRequest();
            }
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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
