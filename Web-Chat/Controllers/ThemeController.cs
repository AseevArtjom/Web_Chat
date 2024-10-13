using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Web_Chat.Models;

namespace Web_Chat.Controllers
{
    [Authorize]
    public class ThemeController : Controller
    {
        public ThemeController()
        {
            
        }

        [HttpPost]
        public ActionResult ChangeTheme([FromBody] ThemeChangeRequest request)
        {
            if (request.Theme == "dark")
            {
                Response.Cookies.Append("theme", "dark");
            }
            else
            {
                Response.Cookies.Append("theme", "light");
            }

            return Ok();
        }

        public class ThemeChangeRequest
        {
            public string Theme { get; set; }
        }

    }
}
