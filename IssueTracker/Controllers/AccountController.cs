using Auth0.AspNetCore.Authentication;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IssueTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly IssueTrackerContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public AccountController(IssueTrackerContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task Login(string returnUrl = "/")
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
        [Authorize]
        public async Task Logout()
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(Url.Action("Index", "Home")!)
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
        }
        [Authorize]
        public IActionResult Profile()
        {
            var Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var _profile = (_context.User!).FirstOrDefault(m => m.EmailAddress == Email)!;
            ////HttpContext.Response.Cookies.Append("user_name", _profile.Name);
            //if (_profile == null)
            //{
                
            //    User NewProfile = new User();
            //    NewProfile.EmailAddress = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value!;
            //    NewProfile.ProfileImage = "default.png"; /*User.FindFirst(c => c.Type == "picture")?.Value;*/
            //    NewProfile.Name = (User.Identity!).Name!;
            //    NewProfile.PhoneNumber = "Unknown";
            //    _context.Add(NewProfile);
            //    _context.SaveChanges();
            //    return View(NewProfile);
            //}
            return View(_profile);


        }

        [Authorize] // GET
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return Redirect("/404");
            }
            // If the logged in user is different than the profile owner return Something??
            var Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var _profile = (_context.User!).FirstOrDefault(m => m.EmailAddress == Email)!;
            if (id != _profile.Id)
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            var userr = (_context.User!).Find(id);
            if (userr == null)
            {
                return Redirect("/404");
            }
            return View(userr);

        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmailAddress,Name, ProfileImage, PhoneNumber, ImageFile")] User userprofile)
        {
            if (id != userprofile.Id)
            {
                return RedirectToAction("AccessDenied", "Error");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(userprofile.ImageFile != null)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(userprofile.ImageFile.FileName);
                        string extension = Path.GetExtension(userprofile.ImageFile.FileName);
                        string ff = Path.GetFileName(userprofile.ImageFile.FileName);
                        string ImageName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(_hostingEnvironment.WebRootPath + "\\Image\\", ImageName);

                        userprofile.ProfileImage = ImageName;
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await userprofile.ImageFile.CopyToAsync(fileStream);
                        }
                    }

                    ViewData["user_image"] = userprofile.ProfileImage;
                    ViewData["user_name"] = userprofile.Name;
                    HttpContext.Response.Cookies.Append("user_image", userprofile.ProfileImage);
                    HttpContext.Response.Cookies.Append("user_name", userprofile.Name);
                    _context.Update(userprofile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(userprofile.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Profile changes saved.";
                return RedirectToAction(nameof(Profile));
                
            }
            return View(userprofile);
        }
        private bool UserExists(int id)
        {
            return (_context.User!).Any(e => e.Id == id)!;
        }
    }
}
