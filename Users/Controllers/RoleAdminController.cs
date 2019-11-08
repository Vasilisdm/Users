using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Users.Models;

namespace Users.Controllers
{
    public class RoleAdminController : Controller
    {
        private RoleManager<IdentityRole> _roleManager;
        private UserManager<AppUser> _userManager;


        public RoleAdminController(RoleManager<IdentityRole> roleMgr, UserManager<AppUser> usrMgr)
        {
            _roleManager = roleMgr;
            _userManager = usrMgr;
        }

        public ViewResult Index() => View(_roleManager.Roles);


        public IActionResult Create() => View();


        [HttpPost]
        public async Task<IActionResult> Create([Required]string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(name));

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(name);
        }


        public async Task<IActionResult> Edit(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            List<AppUser> members = new List<AppUser>();
            List<AppUser> nonMembers = new List<AppUser>();

            foreach (AppUser user in _userManager.Users)
            {
                var list = await _userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }

            return View(new RoleEditModel
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }    
    

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);

            if (role!=null)
            {
                IdentityResult result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "No role found!");
            }

            return View("Index", _roleManager.Roles);
        }


        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}
