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
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        [HttpGet]
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
        [Authorize(Roles = "CompanyAdministrator")]
        public ActionResult<Company> GetCompany(int id)
        {
            if (User.HasClaim("CompanyId",id.ToString())==true)
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (User.HasClaim("CompanyId",id.ToString()) == true || User.IsInRole("Administrator"))
            {
                _context.Entry(company).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
           
            
            return StatusCode(404, "Belirtilen şirket bulunamadı veya erişim izniniz yok.");

        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult PostCompany(Company company, string password)
        {
            //string res = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ApplicationUser applicationUser = new ApplicationUser();
            Claim claim;

            _context.Companies.Add(company);
            _context.SaveChanges();

            applicationUser.CompanyId = company.Id;
            applicationUser.Email = company.EMail;
            applicationUser.Name = "Yeni";
            applicationUser.PhoneNumber = company.Phone;
            applicationUser.RegisterDate = DateTime.Today;
            applicationUser.StateId = 1;
            applicationUser.UserName = "Administrator" + company.Id.ToString();

            var result = _userManager.CreateAsync(applicationUser, password).Result;

            claim = new Claim("CompanyId", company.Id.ToString());

            _userManager.AddClaimAsync(applicationUser, claim).Wait();

            _userManager.AddToRoleAsync(applicationUser, "CompanyAdministrator").Wait();

            return Ok();


        }

        // DELETE: api/Companies/5
        [Authorize(Roles = "Administrator,CompanyAdministrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            if (User.HasClaim("CompanyId",id.ToString()) || User.IsInRole("Administrator"))
            {
                var company = _context.Companies!.FindAsync(id).Result;
                company.StateId = 0;
                _context.Companies.Update(company);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            return StatusCode(404, "company bulunamadı.");


        }

        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
