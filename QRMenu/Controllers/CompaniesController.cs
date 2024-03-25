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
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CompaniesController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Companies
        [HttpGet]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
          if (_context.Companies == null)
          {
              return NotFound();
          }
            return await _context.Companies.ToListAsync();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public ActionResult<Company> GetCompany(int id)
        {
            if (User.HasClaim("CompanyId",id.ToString())==true || User.IsInRole("Administrator"))
            {
                if (_context.Companies == null)
                {
                    return NotFound();
                }
                var company = _context.Companies.FindAsync(id).Result;

                if (company == null)
                {
                    return NotFound();
                }

                return company;
            }
            return NotFound();         
        }

        // PUT: api/Companies/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (User.HasClaim("CompanyId", id.ToString()) == true || User.IsInRole("Administrator"))
            {
                _context.Companies.Update(company);
                _context.SaveChanges();
                return NoContent();
            }
            return StatusCode(404, "Belirtilen şirket bulunamadı veya erişim izniniz yok.");
        }

        // POST: api/Companies
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult PostCompany(Company company, string userName, string password)
        {
            if (User.IsInRole("Administrator")==false)
            {
                return Unauthorized();
            }
            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;

            _context.Companies.Add(company);
            _context.SaveChanges();

            applicationUser.CompanyId = company.Id;
            applicationUser.Email = company.EMail;
            applicationUser.Name = company.Name;
            applicationUser.PhoneNumber = company.Phone;
            applicationUser.RegisterDate = DateTime.Now;
            applicationUser.StateId = 1;
            applicationUser.UserName = userName;

            _userManager.CreateAsync(applicationUser, password).Wait();

            claim = new Claim("CompanyId", company.Id.ToString());

            _userManager.AddClaimAsync(applicationUser, claim).Wait();

            _userManager.AddToRoleAsync(applicationUser, "CompanyAdministrator").Wait();

            return Ok();
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (User.HasClaim("CompanyId",id.ToString()) || User.IsInRole("Administrator"))
            {
                var company = _context.Companies!.FindAsync(id).Result;
                company!.StateId = 0;
                _context.Companies.Update(company);
                var restaurants = _context.Restaurants.Where(r => r.CompanyId == id);
                foreach (Restaurant restaurant in restaurants)
                {
                    restaurant.StateId = 0;
                    _context.Restaurants.Update(restaurant);

                    var categories = _context.Categories.Where(c => c.RestaurantId==restaurant.Id);
                    foreach (Category category in categories)
                    {
                        category.StateId = 0;
                        _context.Categories.Update(category);

                        var foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                        foreach(Food food in foods)
                        {
                            food.StateId = 0;
                            _context.Foods.Update(food);
                        }
                    }
                }
                await _context.SaveChangesAsync();

                return Content("Company and the all layer related with company has been deleted.");
            }
            return NotFound();
        }
    }
}
