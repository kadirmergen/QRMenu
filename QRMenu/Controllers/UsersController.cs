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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationContext context, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetApplicationUsers()
        {
            if (_signInManager.UserManager == null)
            {
                return NotFound();
            }
            return await _signInManager.UserManager.Users.ToListAsync();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public ActionResult<ApplicationUser> GetApplicationUser(string id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var applicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;

            if (applicationUser == null)
            {
                return NotFound();
            }

            return applicationUser;
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles ="CompanyAdministrator")]
        [HttpPut("{id}")]
        public OkResult PutApplicationUser(ApplicationUser applicationUser)
        {
            ApplicationUser existApplicationUser= _signInManager.UserManager.FindByIdAsync(applicationUser.Id).Result;

            existApplicationUser.Email = applicationUser.Email;
            existApplicationUser.Name = applicationUser.Name;
            existApplicationUser.UserName = applicationUser.UserName;
            existApplicationUser.PhoneNumber = applicationUser.PhoneNumber;
            existApplicationUser.StateId = applicationUser.StateId;

            _signInManager.UserManager.UpdateAsync(existApplicationUser);
            return Ok();
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "CompanyAdministrator")]
        [HttpPost]
        public string PostApplicationUser(ApplicationUser applicationUser , string password)
        {
            _signInManager.UserManager.CreateAsync(applicationUser, password).Wait();
            return applicationUser.Id;
        }

        // DELETE: api/Companies/5
        [Authorize(Roles = "CompanyAdministrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteApplicationUser(string id)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;

            if (applicationUser==null)
            {
                return NotFound();
            }
            applicationUser.StateId = 0;
            _signInManager.UserManager.UpdateAsync(applicationUser).Wait();

            return Ok();

        }

        [HttpPost("Login")]
        public bool Login(string userName, string password)
        {
            Microsoft.AspNetCore.Identity.SignInResult signInResult;
            ApplicationUser applicationUser= _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser==null)
            {
                return false;
            }
            signInResult = _signInManager.PasswordSignInAsync(userName, password, isPersistent: false, lockoutOnFailure: false).Result;
            return signInResult.Succeeded;

        }

        [HttpPost("ResetPassword")]
        public void ResetPassword(string userName, string password)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                return;
            }
            _signInManager.UserManager.RemovePasswordAsync(applicationUser).Wait();
            _signInManager.UserManager.AddPasswordAsync(applicationUser, password);
        }

        [Authorize(Roles ="CompanyAdministrator")]
        [HttpPost("AssignRole")]
        public void AssignRole(string userId, string roleId)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByIdAsync(userId).Result;
            IdentityRole identityRole = _roleManager.FindByIdAsync(roleId).Result;

            _signInManager.UserManager.AddToRoleAsync(applicationUser, identityRole.Name).Wait();
        }


        private bool CompanyExists(int id)
        {
            return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
