using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Users.Models;
using Microsoft.AspNetCore.Authorization;

namespace Users.Controllers
{
    [Authorize(Roles = "Admins")]
    public class AdminController : Controller
    {

        private UserManager<AppUser> userManager;
        private IUserValidator<AppUser> _userValidator;
        private IPasswordValidator<AppUser> _passwordValidator;
        private IPasswordHasher<AppUser> _passwordHasher;


        public AdminController(UserManager<AppUser> usrMgr,
            IUserValidator<AppUser> userValid,
            IPasswordValidator<AppUser> passValid,
            IPasswordHasher<AppUser> passwordHash)
        {
            userManager = usrMgr;
            _userValidator = userValid;
            _passwordValidator = passValid;
            _passwordHasher = passwordHash;
        }


        public ViewResult Index() => View(userManager.Users);


        public ViewResult Create() => View();


        [HttpPost]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email
                };

                IdentityResult result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

            }
            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);

            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Edit(string id, string email, string password)
        {
            AppUser user = await userManager.FindByIdAsync(id);

            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail = await _userValidator.ValidateAsync(userManager, user);

                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await _passwordValidator.ValidateAsync(userManager, user ,password);

                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }

                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded && password != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index"); 
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User not found");
            }
            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            AppUser user = await userManager.FindByIdAsync(Id);

            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);

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
                ModelState.AddModelError("", "User Not Found");
            }
            return View("Index", userManager.Users);
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
