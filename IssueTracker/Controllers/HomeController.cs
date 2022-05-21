using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList;


namespace IssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IssueTrackerContext _context;
        public HomeController(ILogger<HomeController> logger, IssueTrackerContext context)
        {
            _logger = logger;
            _context = context;
        }
        private void createUserIfFirstLogin(string Email)
        {
            var _profile = (_context.User!).FirstOrDefault(m => m.EmailAddress == Email)!;
            if (_profile == null)
            {

                User NewProfile = new User();
                NewProfile.EmailAddress = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value!;
                NewProfile.ProfileImage = "default.png";
                NewProfile.Name = (User.Identity!).Name!;
                NewProfile.PhoneNumber = "Unknown";
                _context.Add(NewProfile);
                _context.SaveChanges();
            }
        }
        public IActionResult Index(int pgp = 1, int pgo = 1, int pgf = 1, int pgc = 1)
        {
            if (User!.Identity!.IsAuthenticated)
            {
                createUserIfFirstLogin(User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value);
                List<string> CurrentUrl = new List<string>();
                if (!String.IsNullOrEmpty(HttpContext.Request.Query["pgp"]))
                {
                    CurrentUrl.Add(("pgp=" + HttpContext.Request.Query["pgp"]));
                }
                if (!String.IsNullOrEmpty(HttpContext.Request.Query["pgo"]))
                {
                    CurrentUrl.Add(("pgo=" + HttpContext.Request.Query["pgo"]));
                }
                if (!String.IsNullOrEmpty(HttpContext.Request.Query["pgf"]))
                {
                    CurrentUrl.Add(("pgf=" + HttpContext.Request.Query["pgf"]));
                }
                if (!String.IsNullOrEmpty(HttpContext.Request.Query["pgc"]))
                {
                    CurrentUrl.Add(("pgc=" + HttpContext.Request.Query["pgc"]));
                }


                ViewBag.CurrentUrl = CurrentUrl;
                // Needed for displaying/creating projects, issues in the main page.
                ViewBag.Projects = _context.Project!.Include(p => p.Users).ToPagedList(pgp, 2);
                ViewBag.ProjectCurrentPage = pgp;
                ViewBag.ProjectMaxPage = _context.Project!.Include(p => p.Users).ToPagedList(pgp, 2).PageCount;
                // Open Issues
                ViewBag.OpenIssues = _context.Issue!.Where(i => i.Status == StatusType.Opened || i.Status == StatusType.Reopened).ToPagedList(pgo, 2);
                ViewBag.OpenIssueCurrentPage = pgo;
                ViewBag.OpenIssueMaxPage = _context.Issue!.Where(i => i.Status == StatusType.Opened || i.Status == StatusType.Reopened).ToPagedList(pgo, 2).PageCount;
                // Fixed Issues
                ViewBag.FixedIssues = _context.Issue!.Where(i => i.Status == StatusType.Fixed).ToPagedList(pgf, 2);
                ViewBag.FixedIssueCurrentPage = pgf;
                ViewBag.FixedIssueMaxPage = _context.Issue!.Where(i => i.Status == StatusType.Fixed).ToPagedList(pgf, 2).PageCount;
                // Closed Issues
                ViewBag.ClosedIssues = _context.Issue!.Where(i => i.Status == StatusType.Closed).ToPagedList(pgc, 2);
                ViewBag.ClosedIssueCurrentPage = pgc;
                ViewBag.ClosedIssueMaxPage = _context.Issue!.Where(i => i.Status == StatusType.Closed).ToPagedList(pgc, 2).PageCount;
                // Get user and project data for selection
                ViewBag.Users = new SelectList(_context.User!.ToList(), "Id", "Name");
                ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName");

                // To save the user profile image, name and email between requests
                //var Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
                //var _profile = _context.User!.FirstOrDefault(m => m.EmailAddress == Email);
                //if(_profile != null)
                //{
                //    ViewData["user_image"] = _profile!.ProfileImage;
                //    ViewData["user_name"] = _profile!.Name;
                //    ViewData["user_email"] = _profile!.EmailAddress;
                //}
                //else
                //{
                //    ViewData["user_image"] = "default.png";
                //    ViewData["user_name"] = "Name Surname";
                //    ViewData["user_email"] = Email;
                //}

                //HttpContext.Response.Cookies.Append("user_image", ViewData["user_image"].ToString());
                //HttpContext.Response.Cookies.Append("user_name", ViewData["user_name"].ToString());
                //HttpContext.Response.Cookies.Append("user_email", ViewData["user_email"].ToString());
                return View();
            }
            return RedirectToAction("Login", "Account");
            //return Redirect("Home/Login");
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}