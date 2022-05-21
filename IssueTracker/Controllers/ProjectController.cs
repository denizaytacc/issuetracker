using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;

namespace IssueTracker.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IssueTrackerContext _context;

        public ProjectController(IssueTrackerContext context)
        {
            _context = context;
        }



        // GET: Project
        public async Task<IActionResult> Index()
        {
              return _context.Project != null ? 
                          View(await _context.Project.ToListAsync()) :
                          Problem("Entity set 'IssueTrackerContext.Project'  is null.");
        }


        // GET: Project/Details/5
        public async Task<IActionResult> Details(int? id, int pg = 1)
        {
            if (id == null || _context.Project == null)
            {
                return Redirect("/404");
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return Redirect("/404");
            }

            List<User> SelectedUsers = new List<User>();
            List<User> UnSelectedUsers = new List<User>();
            var AllUsers = _context.User!.Include(x => x.Projects);
            foreach (var usr in AllUsers)
            {
                foreach (var pr in usr.Projects)
                {
                    if (SelectedUsers.Contains(usr))
                    {
                        continue;
                    }
                    if (pr.Id == id)
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
            var projectIssues = _context.Issue!.Include(x => x.Project).Where(y => y.ProjectId == id);
            ViewBag.ProjectIssues = projectIssues.ToPagedList(pg, 5);
            ViewBag.CurrentPage = pg;
            ViewBag.MaxPage = projectIssues.ToPagedList(pg, 5).PageCount;
            ViewBag.SelectedUsers = SelectedUsers;
            ViewBag.UnSelectedUsers = new SelectList(UnSelectedUsers, "Id", "Name");
            ViewBag.Users = new SelectList(_context.User!.ToList(), "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project.Where(x => x.Id == id), "Id", "ProjectName");
            return View(project);
        }


        [Authorize(Roles="Admin")]
        // GET: Project/Create
        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_context.User!.ToList(), "Id", "Name");
            return View();
        }

        // POST: Project/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectName,ProjectDescription,Progress")] Project project)
        {
            var UserIds = this.HttpContext.Request.Form["ProjectUsers"].ToString().Split(',');

            if (ModelState.IsValid)
            {

                foreach (var userId in UserIds)
                {
                    if (userId != "")
                    {
                        var usr = _context.User!.Find(int.Parse(userId));
                        project.Users!.Add(usr!);
                    }
                }
                _context.Add(project);
                await _context.SaveChangesAsync();
                TempData["success"] = "Project created successfully!";
                return RedirectToAction("Details", "Project", new {id = project.Id});
                //return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("Index", "Home");
            //return View(project); if fails it return to Issue/Create
        }

        [Authorize(Roles = "Admin")]
        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return Redirect("/404");
            }

            //_context.Project.Include(p => p.Users);
            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return Redirect("/404");
            }
            List<User> SelectedUsers = new List<User>();
            List<User> UnSelectedUsers = new List<User>();
            var AllUsers = _context.User!.Include(x => x.Projects);
            foreach (var usr in AllUsers)
            {
                foreach (var pr in usr.Projects)
                {
                    if (SelectedUsers.Contains(usr))
                    {
                        continue;
                    }
                    if (pr.Id == id)
                    {
                        SelectedUsers.Add(usr);
                    }
                }
            }
            foreach(var usr in AllUsers)
            {
                if (!SelectedUsers.Contains(usr))
                {
                    UnSelectedUsers.Add(usr);
                }
            }
            ViewBag.SelectedUsers = SelectedUsers;
            ViewBag.UnSelectedUsers = new SelectList(UnSelectedUsers, "Id", "Name");
            return View(project);
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectName,ProjectDescription,Progress")] Project project)
        {
            var UserIds = this.HttpContext.Request.Form["ProjectUsers"].ToString().Split(',');
            if (id != project.Id)
            {
                return Redirect("/404");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dbProject = _context.Project!.Include(u => u.Users).First(p => p.Id == id);
                    dbProject.ProjectName = this.HttpContext.Request.Form["ProjectName"].ToString();
                    dbProject.ProjectDescription = this.HttpContext.Request.Form["ProjectDescription"].ToString();
                    dbProject.Progress = int.Parse(this.HttpContext.Request.Form["Progress"].ToString());
                    foreach (var usr in dbProject.Users!)
                    {
                        dbProject.Users.Remove(usr);
                    }
                    _context.Update(dbProject);
                    await _context.SaveChangesAsync();
                    foreach (var userId in UserIds)
                    {
                        if (userId != "")
                        {
                            var usr = _context.User!.Find(int.Parse(userId));
                            dbProject.Users.Add(usr!);

                        }

                    }
                    _context.Update(dbProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["success"] = "Changes saved!";
                return RedirectToAction("Details", "Project", new {id = id});
            }
            return View(project);
        }

        [Authorize(Roles = "Admin")]
        
        // POST: Project/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'IssueTrackerContext.Project'  is null.");
            }
            var project = await _context.Project.FindAsync(id);
            if (project != null)
            {
                _context.Project.Remove(project);
            }
            
            await _context.SaveChangesAsync();
            TempData["success"] = "Project was deleted successfully.";
            return RedirectToAction("Index", "Home");
        }

        private bool ProjectExists(int id)
        {
          return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
