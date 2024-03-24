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
            _context.Entry(food).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok("Food updated!");
        }

        // POST: api/Foods
        [HttpPost]
        [Authorize(Roles ="RestaurantAdministrator, CompanyAdministrator")]
        public async Task<ActionResult<Food>> PostFood(Food food)
        {
            _context.Foods.Add(food);
            await _context.SaveChangesAsync();
            return Ok("Food added!");
        }

        // DELETE: api/Foods/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFood(int id)
        {
            var food = await _context.Foods!.FindAsync(id);
            food!.StateId = 0;
            _context.Foods.Update(food);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
