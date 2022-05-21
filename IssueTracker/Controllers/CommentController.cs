using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class CommentController : Controller
    {
        private readonly IssueTrackerContext _context;
        public CommentController( IssueTrackerContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Post([Bind("Text")] Comment comment, int id, string Text)
        {
            var commentText = HttpContext.Request.Form["Text"];
            comment.Text = commentText;
            comment.IssueId = id;
            var realIssue = _context.Issue!.Find(id);
            
            var useremail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            var user = (_context.User!).First(u => u.EmailAddress == useremail);
            comment.UserId = user.Id;
            _context.Add(comment);
            _context.SaveChanges();
            (realIssue!).Comments!.Add(comment);
            _context.Update(realIssue);
            _context.SaveChanges();
            TempData["success"] = "Comment was posted!";
            return RedirectToAction("Details", "Issue", new { id = id});
        }
        public IActionResult Delete(int? id)
        {

            var comment = (_context.Comment!).FirstOrDefault(m => m.Id == id);
            var commentUser = (_context.User!).FirstOrDefault(m => m.Id == comment!.UserId);
            var commentIssue = (_context.Issue!).Include(c => c.Comments).FirstOrDefault(m => m.Id == comment!.IssueId);

            // Check if the owner of the comment is the same as the current login
            var LoggerEmail = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            if (commentUser!.EmailAddress != LoggerEmail)
            {
                return NotFound();
            }
            commentIssue!.Comments!.Remove(comment!);
            _context!.Comment!.Remove(comment!);
            _context.Update(comment!);
            _context!.Update(commentIssue);
            _context.SaveChanges();
            TempData["success"] = "Comment was deleted successfully!";
            return RedirectToAction("Details", "Issue", new { id = commentIssue.Id });
        }
    }
}
