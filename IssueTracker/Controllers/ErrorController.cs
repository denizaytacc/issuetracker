using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    public class ErrorController : Controller
    {
        [Route("{*url}", Order=2)]
        [Route("404")]
        public IActionResult NotFound()
        {
            return View();
        }
        [Route("403")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
