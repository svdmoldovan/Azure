using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ConfArch.Web.Controllers
{
    public class AccountController: Controller
    {
        public IActionResult Login() => Challenge(new AuthenticationProperties { RedirectUri = "/" });
    }
}
