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
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FoodsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Foods
        [HttpGet]
        public ActionResult<IEnumerable<Food>> GetFoods()
        {
            return  _context.Foods!.ToList();
        }

        // GET: api/Foods/5
        [HttpGet("{id}")]
        [Authorize(Roles ="RestaurantAdministrator")]
        public async Task<ActionResult<Food>> GetFood(int id)
        {
            var food = await _context.Foods!.FindAsync(id);
            if (food != null)
            {
                return food; 
            }
            return NotFound("There isnt any food with that id.");
        }

        // PUT: api/Foods/5
        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantAdministrator, CompanyAdministrator")]
        public async Task<IActionResult> PutFood(int id, Food food)
        {
            if (User.HasClaim("RestaurantId",id.ToString()) || User.HasClaim("CompanyId",id.ToString()))
            {
                _context.Entry(food).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return Ok("Food updated!");
        }

        // POST: api/Foods
        [HttpPost]
        [Authorize(Roles ="RestaurantAdministrator, CompanyAdministrator")]
        public async Task<ActionResult<Food>> PostFood(Food food, int id)
        {
            if (User.HasClaim("RestaurantId", id.ToString()) || User.HasClaim("CompanyId", id.ToString()))
            {
                _context.Foods.Add(food);
                await _context.SaveChangesAsync();
            }
            return Ok("Food added!");
        }
        [HttpPost("UploadImage/{foodId}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> UploadImage(int foodId, IFormFile file)
        {
            var food = await _context.Foods.FindAsync(foodId);
            if (food == null)
            {
                return NotFound("Food cannot found");
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

            food.Image = $"/images/{fileName}";
            _context.Foods.Update(food);
            await _context.SaveChangesAsync();

            return Ok("Image uploaded.");
        }

        [HttpPut("EditImage/{foodId}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> EditImage(int foodId, IFormFile file)
        {
            var food = await _context.Categories.FindAsync(foodId);
            if (food == null)
            {
                return NotFound("Category cannot found.");
            }

            if (!string.IsNullOrEmpty(food.Image))
            {
                var imagePath = Path.Combine("wwwroot", food.Image.TrimStart('/'));
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

            food.Image = $"/images/{fileName}";
            _context.Categories.Update(food);
            await _context.SaveChangesAsync();

            return Ok("Image added.");
        }

        // DELETE: api/Foods/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            if (User.HasClaim("RestaurantId",id.ToString()))
            {
                var food = await _context.Foods!.FindAsync(id);
                food!.StateId = 0;
                _context.Foods.Update(food);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
