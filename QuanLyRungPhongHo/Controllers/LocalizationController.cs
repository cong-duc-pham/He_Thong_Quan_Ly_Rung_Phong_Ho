using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyRungPhongHo.Controllers
{
    public class LocalizationController : Controller
    {
        [HttpGet] 
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
            {
                culture = "vi";
            }

            // Set cookie với culture đã chọn
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/",
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Validate và redirect
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            return LocalRedirect(returnUrl);
        }
    }
}
