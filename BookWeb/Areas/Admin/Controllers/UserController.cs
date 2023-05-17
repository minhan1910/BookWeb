using BookWeb.DataAccess.Data;
using BookWeb.DataAccess.Repository.IRepository;
using BookWeb.Models;
using BookWeb.Models.ViewModels;
using BookWeb.Utility;
using BookWeb.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Linq;

namespace BookWeb.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<IdentityRole> _roles;

        public UserController(ApplicationDbContext db, 
                              RoleManager<IdentityRole> roleManager, 
                              UserManager<ApplicationUser> userManager, 
                              IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _roles = _roleManager.Roles.ToList();
        }

        public IActionResult Index()
        {            
            return View();
        }

        [HttpGet]
        public IActionResult RoleManagement(string userId)
        {
            ApplicationUser? user = _db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)       
            {
                return NotFound();
            }

            List<IdentityUserRole<string>> userRoles = _db.UserRoles.ToList();
            List<Company> companies = _db.Companies.ToList();

            IdentityUserRole<string> userRoleOfCurrentUser = userRoles.Where(r => r.UserId == user.Id).First();
            Company? companyOfCurrentUser = companies.FirstOrDefault(c => c.Id == user.CompanyId);

            user.Name = user.Name + $" ({_roles.First(r => r.Id == userRoleOfCurrentUser.RoleId).Name})";

            UserVM userVm = new()
            {
                ApplicationUser = user,
                RoleId = _roles.First(r => r.Id == userRoleOfCurrentUser.RoleId)!.Id,
                CompanyId = companyOfCurrentUser == null ? string.Empty : companyOfCurrentUser.Id.ToString(),
                Companies = companies,
                Roles = _roles
            };


            return View(userVm);
        }

        [HttpPost]
        public IActionResult RoleManagement(UserVM userVm)
        {
            if (userVm.RoleId == null)
            {
                return NotFound();
            }

            ApplicationUser? userFromDb = _userManager.FindByIdAsync(userVm.UserID).GetAwaiter().GetResult();

            if (userFromDb == null)
            {
                return NotFound();
            }

            UpdateRoleUser(userFromDb, userVm.RoleId).GetAwaiter().GetResult();

            UpsertCompanyUser(userVm.CompanyId, userFromDb);

            TempData["success"] = "Update Role Successfully!";

            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateRoleUser(ApplicationUser userFromDb, string? newRoleId) 
        {            
            IdentityUserRole<string>? oldUserRoleFromDb = _db.UserRoles.Where(r => r.UserId == userFromDb.Id).FirstOrDefault();

            if (oldUserRoleFromDb != null &&
                newRoleId != null &&
                newRoleId != oldUserRoleFromDb.RoleId)
            {
                var oldRole = _roles.FirstOrDefault(r => r.Id == oldUserRoleFromDb.RoleId);
                var newRole = _roles.FirstOrDefault(r => r.Id == newRoleId);

                if (oldRole != null && newRole != null)
                {
                    await _userManager.RemoveFromRoleAsync(userFromDb, oldRole.Name!);
                    await _userManager.AddToRoleAsync(userFromDb, newRole.Name!);
                }
            }
        }

        private void UpsertCompanyUser(string? newCompanyId, ApplicationUser userFromDb)
        {            
            IdentityUserRole<string>? oldUserRoleFromDb = _db.UserRoles.Where(r => r.UserId == userFromDb.Id).FirstOrDefault();

            IdentityRole? roleCurrentUser = null;

            if (oldUserRoleFromDb != null)
            {
                roleCurrentUser = _roles.FirstOrDefault(r => r.Id == oldUserRoleFromDb.RoleId);
            }

            if (newCompanyId != null && roleCurrentUser != null && roleCurrentUser.Name == SD.Role_Company)
            {
                var companyIdInt = Int32.Parse(newCompanyId);

                if (userFromDb.CompanyId != null)
                {
                    // update
                    userFromDb.CompanyId = companyIdInt;
                    _db.Users.Update(userFromDb);
                }
                else
                {
                    // create
                    userFromDb.CompanyId = companyIdInt;
                }
                
            }
            else
            {
                userFromDb.CompanyId = null;
            }
            
            _db.SaveChanges();
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {            
            List<ApplicationUser> users = _db.ApplicationUsers.Include(u => u.Company).ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            // Handling display the user has not belong to company 
            foreach (var user in users)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                var role = roles.FirstOrDefault(u => u.Id == roleId);

                user.Role = role.Name;

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }

            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string? id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking"});
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            } else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddDays(1_000);
            }

            _db.SaveChanges();

            return Json(new { success = true, message = "Operation successfully" });
        }

        #endregion
    }
}
