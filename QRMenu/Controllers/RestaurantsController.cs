using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenu.Data;
using QRMenu.Models;

namespace QRMenu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RestaurantsController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Restaurants
        [HttpGet]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            return await _context.Restaurants.ToListAsync();
        }

        // GET: api/Restaurants/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant != null)
            {
                return restaurant; 
            }
            return NotFound();
        }

        // PUT: api/Restaurants/5
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdministrator, RestaurantAdministrator")]
        public async Task<IActionResult> PutRestaurant(int id, Restaurant restaurant)
        {
            if (User.HasClaim("Restaurant",id.ToString()) || User.IsInRole("CompanyAdministrator"))
            {
                _context.Entry(restaurant).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            if (id != restaurant.Id)
            {
                return BadRequest("There isnt any restaurant with that id");
            }
            return Ok("Restaurant updated.");
        }

        // POST: api/Restaurants
        [HttpPost]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<ActionResult<Restaurant>> PostRestaurant(Restaurant restaurant,string userName, string password)
        {
            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;
            
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            applicationUser.Name = restaurant.BranchName;
            applicationUser.PhoneNumber = restaurant.PhoneNumber;
            applicationUser.CompanyId = restaurant.CompanyId;
            applicationUser.StateId = 1;
            applicationUser.UserName = userName;

            var result = _userManager.CreateAsync(applicationUser, password).Result;
            
            claim = new Claim("Restaurant", restaurant.Id.ToString());

            _userManager.AddClaimAsync(applicationUser, claim).Wait();

            _userManager.AddToRoleAsync(applicationUser, "RestaurantAdministrator").Wait();

            return Ok("Restaurant added.");
        }

        // DELETE: api/Restaurants/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdministrator")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants!.FindAsync(id);
            restaurant!.StateId = 0;
            _context.Restaurants.Update(restaurant);

            var categories = _context.Categories.Where(c => c.RestaurantId == restaurant.Id);
            foreach (Category category in categories)
            {
                category.StateId = 0;
                _context.Categories.Update(category);

                var foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                foreach (Food food in foods)
                {
                    food.StateId = 0;
                    _context.Foods.Update(food);
                }
            }
            await _context.SaveChangesAsync();

            return Content("Restaurant has been deleted.");
        }
    }
}
