using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Data;
using IssueTracker.Models;
using System.Security.Claims;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace IssueTracker.Controllers
{
    public class IssueController : Controller
    {
        private readonly IssueTrackerContext _context;

        public IssueController(IssueTrackerContext context)
        {
            _context = context;
        }

        // GET: Issue
        public async Task<IActionResult> Index(int pg = 1, int pgm = 1)
        {
            var Email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var currentuser = await _context.User!.FirstOrDefaultAsync(m => m.EmailAddress == Email);
            var all_issues = _context.Issue!.Include(i => i.Users).ToList();
            var user_issues = new List<Issue>();
            foreach (var an_issue in all_issues)
            {
                if (an_issue.Users!.Contains(currentuser!)){
                    user_issues.Add(an_issue);

                }
            }
            List<string> CurrentUrl = new List<string>();
            if (!String.IsNullOrEmpty(HttpContext.Request.Query["pg"]))
            {
                CurrentUrl.Add(("pg=" + HttpContext.Request.Query["pg"]));
            }
            if (!String.IsNullOrEmpty(HttpContext.Request.Query["pgm"]))
            {
                CurrentUrl.Add(("pgm=" + HttpContext.Request.Query["pgm"]));
            }
            ViewBag.CurrentUrl = CurrentUrl;
            ViewBag.Users = new SelectList(_context.User, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName");
            ViewBag.UserIssues = user_issues.ToPagedList(pg, 5);
            ViewBag.CurrentPage = pg;
            ViewBag.MaxPage = user_issues.ToPagedList(pg, 5).PageCount;
            ViewBag.Members = _context.Project!.Include(p => p.Users).Where(x => x.Users!.Contains(currentuser!)).ToPagedList(pgm, 1);
            ViewBag.PagingUrl = (pg != 1);
            ViewBag.MembersCurrentPage = pgm;
            ViewBag.MembersMaxPage = _context.Project!.Include(p => p.Users).Where(x => x.Users!.Contains(currentuser!)).ToPagedList(pgm, 1).PageCount;
            return View();
        }

        // GET: Issue/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Issue == null)
            {
                return Redirect("/404");
            }

            var issue = await _context.Issue!
                .Include(i => i.Project!)
                .Include(i => i.Comments!).ThenInclude(i=>i.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (issue == null)
            {
                return Redirect("/404");
            }
            List<User> SelectedUsers = new List<User>();
            List<User> UnSelectedUsers = new List<User>();
            var AllUsers = _context.User!.Include(x => x.Issues);
            foreach (var usr in AllUsers)
            {
                foreach (var iss in usr.Issues)
                {
                    if (SelectedUsers.Contains(usr))
                    {
                        continue;
                    }
                    if (iss.Id == id)
                    {
                        SelectedUsers.Add(usr);
                    }
                }
            }
            foreach (var usr in AllUsers)
            {
                if (!SelectedUsers.Contains(usr))
                {
                    UnSelectedUsers.Add(usr);
                }
            }
            ViewBag.SelectedUsers = SelectedUsers;
            ViewBag.UnSelectedUsers = new SelectList(UnSelectedUsers, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName", issue.ProjectId);


            return View(issue);
        }


        // POST: Issue/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IssueName,Description,EstimatedTimeToComplete,Priority,Status,ProjectId")] Issue issue)
        {
            if(!(User.IsInRole("Admin") || User.IsInRole("Admin")))
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            var UserIds = this.HttpContext.Request.Form["IssueUsers"].ToString().Split(',');
            if (ModelState.IsValid)
            {
                foreach(var UserId in UserIds)
                {
                    if (UserId != ""){
                        var usr = _context.User!.Find(int.Parse(UserId));
                        issue.Users!.Add(usr!);
                    }

                }
                if (issue.Users!.Count == 0 && (issue.Status != StatusType.Reopened || issue.Status != StatusType.Opened))
                {
                    TempData["warning"] = "Since no users were appointed the issue type was converted to Opened.";
                    issue.Status = StatusType.Opened;
                }
                if (issue.Users.Count > 0 && (issue.Status == StatusType.Reopened || issue.Status == StatusType.Opened))
                {
                    TempData["warning"] = "Since there are user(s), the issue type was converted to Assigned.";
                    issue.Status = StatusType.Assigned;
                }
                _context.Add(issue);
                await _context.SaveChangesAsync();
                TempData["success"] = "Issue created successfully!";
                return RedirectToAction("Details", "Issue", new { id = issue.Id });
            }
            ViewBag.Users = new SelectList(_context.User, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName", issue.ProjectId);
            TempData["error"] = "Model state isn't valid";
            return View(issue);
        }

        // GET: Issue/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Issue == null)
        //    {
        //        return NotFound();
        //    }

        //    //var issue = _context.Issue.Include(x => x.Users).First(m=>m.Id==id);
        //    var issue = await _context.Issue.FirstAsync(m => m.Id == id);
        //    if (issue == null)
        //    {
        //        return NotFound();
        //    }
        //    List<User> SelectedUsers = new List<User>();
        //    List<User> UnSelectedUsers = new List<User>();
        //    var AllUsers = _context.User!.Include(x => x.Issues);
        //    foreach (var usr in AllUsers)
        //    {
        //        foreach (var iss in usr.Issues)
        //        {
        //            if (SelectedUsers.Contains(usr))
        //            {
        //                continue;
        //            }
        //            if (iss.Id == id)
        //            {
        //                SelectedUsers.Add(usr);
        //            }
        //        }
        //    }
        //    foreach (var usr in AllUsers)
        //    {
        //        if (!SelectedUsers.Contains(usr))
        //        {
        //            UnSelectedUsers.Add(usr);
        //        }
        //    }
        //    ViewBag.SelectedUsers = SelectedUsers;
        //    ViewBag.UnSelectedUsers = new SelectList(UnSelectedUsers, "Id", "Name");

        //    ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName", issue.ProjectId);
        //    return View(issue);
        //}

        // POST: Issue/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IssueName,Description,EstimatedTimeToComplete,Priority,Status,ProjectId, Users")] Issue issue)
        {
            if (!(User.IsInRole("Admin") || User.IsInRole("Admin")))
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            var UserIds = this.HttpContext.Request.Form["IssueUsers"].ToString().Split(',');
            if (id != issue.Id)
            {
                return Redirect("/404");
            }

            if (ModelState.IsValid)
            {
                try
                {

                    var dbIssue = _context.Issue!.Include(u => u.Users).First(p => p.Id == id);
                    dbIssue.IssueName = this.HttpContext.Request.Form["IssueName"].ToString();
                    dbIssue.Description = this.HttpContext.Request.Form["Description"].ToString();
                    dbIssue.EstimatedTimeToComplete = int.Parse(this.HttpContext.Request.Form["EstimatedTimeToComplete"].ToString());
                    dbIssue.Priority = (PriorityType)int.Parse(this.HttpContext.Request.Form["Priority"].ToString());
                    dbIssue.Status = (StatusType)int.Parse(this.HttpContext.Request.Form["Status"].ToString());
                    dbIssue.ProjectId = int.Parse(this.HttpContext.Request.Form["ProjectId"].ToString());
                    foreach (var usr in dbIssue!.Users!)
                    {
                        dbIssue.Users.Remove(usr);
                    }
                    _context.Update(dbIssue);
                    await _context.SaveChangesAsync();
                    foreach (var userId in UserIds)
                    {
                        if (userId != "")
                        {
                            var usr = _context.User!.Find(int.Parse(userId));
                            dbIssue.Users.Add(usr!);

                        }
                        
                    }
                    if (dbIssue.Users.Count == 0 && (dbIssue.Status != StatusType.Reopened || dbIssue.Status != StatusType.Opened))
                    {
                        TempData["warning"] = "Since no users were appointed the issue type was converted to Opened.";
                        dbIssue.Status = StatusType.Opened;
                    }
                    if (dbIssue.Users.Count > 0 && (dbIssue.Status == StatusType.Reopened || dbIssue.Status == StatusType.Opened))
                    {
                        TempData["warning"] = "Since there are user(s), the issue type was converted to Assigned.";
                        dbIssue.Status = StatusType.Assigned;
                    }
                    _context.Update(dbIssue);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IssueExists(issue.Id))
                    {
                        return Redirect("/404");
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Changes saved!";
                return RedirectToAction("Details", "Issue", new { id = id });
            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "ProjectName", issue.ProjectId);

            return RedirectToAction("Details", "Issue", new { id = id});
        }
        public IActionResult Claim(int? id)
        {
            if (id == null)
            {
                return Redirect("/404");
            }
            var dbIssue = _context.Issue!.Include(x => x.Users).First(y => y.Id == id);
            if (dbIssue == null)
            {
                return Redirect("/404");
            }
            if (dbIssue.Users!.Count > 0) {
                TempData["error"] = "The Issue was already claimed by " + dbIssue.Users.First().Name.ToString();
                return RedirectToAction("Index", "Home");
            }
            var userEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var currentUser = _context.User!.FirstOrDefault(m => m.EmailAddress == userEmail);
            dbIssue.Users.Add(currentUser!);
            dbIssue.Status = StatusType.Assigned;
            _context.Update(dbIssue);
            _context.SaveChanges();
            TempData["success"] = "The Issue is claimed by " + currentUser!.Name;
            return RedirectToAction("Details", "Issue", new { id = id });
        }



        // POST: Issue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!(User.IsInRole("Admin") || User.IsInRole("Admin")))
            {
                return RedirectToAction("AccessDenied", "Error");
            }
            if (_context.Issue == null)
            {
                return Problem("Entity set 'IssueTrackerContext.Issue'  is null.");
            }
            var issue = await _context.Issue.FindAsync(id);
            if (issue != null)
            {
                _context.Issue.Remove(issue);
            }
            TempData["success"] = "Issue deleted successfully.";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IssueExists(int id)
        {
          return (_context.Issue?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
