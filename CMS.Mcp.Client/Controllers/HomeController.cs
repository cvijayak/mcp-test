namespace CMS.Mcp.Client.Controllers 
{
    using Contracts.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("mcp/home")]
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")] 
        [Authorize]
        public IActionResult Index(string id = null)
        {
            return RedirectToAction("Index", "Chat");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            var statusCode = HttpContext.Response.StatusCode;
            var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;
            
            if (User.Identity?.IsAuthenticated == true)
            {
                return View("Error", new ErrorViewModel
                {
                    StatusCode = statusCode,
                    Message = exception?.Message ?? "An error occurred while processing your request.",
                    RequestId = HttpContext.TraceIdentifier
                });
            }
            
            return RedirectToAction("Login", "Account");
        }
    }
}