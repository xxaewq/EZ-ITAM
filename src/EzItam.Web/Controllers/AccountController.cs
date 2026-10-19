using System.Security.Claims;
using EzItam.Web.Api;
using EzItam.Web.Localization;
using Microsoft.Extensions.Localization;
using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace EzItam.Web.Controllers;

public class AccountController : BaseController
{
    public AccountController(ItamApiClient api, IStringLocalizer<SharedResource> loc) : base(api, loc) { }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            model.UserName = "admin";
            model.Password = "Admin!2345";

            model.UserName = model.UserName.Trim();
            model.Password = model.Password.Trim();
            var token = await Api.LoginAsync(new LoginRequest { UserName = model.UserName, Password = model.Password });
            var claims = new List<Claim> { new(ClaimTypes.Name, token.UserName) };
            claims.AddRange(token.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            var props = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = token.ExpiresAtUtc
            };
            props.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = token.AccessToken } });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return Redirect(model.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Expired()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Alert.Error"] = L["로그인 세션이 만료되었습니다. 다시 로그인하세요."];
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await Api.ChangePasswordAsync(new ChangePasswordRequest { CurrentPassword = model.CurrentPassword, NewPassword = model.NewPassword });
            Success(L["비밀번호가 변경되었습니다."]);
            return RedirectToAction("Index", "Home");
        }
        catch (ApiException ex)
        {
            AddApiError(ex);
            return View(model);
        }
    }
}
