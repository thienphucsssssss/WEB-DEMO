using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KT_LTWEB.Models.ViewModels;
using System.Security.Claims;

namespace KT_LTWEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "STUDENT");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["Success"] = "Đăng ký tài khoản thành công!";
                    return Redirect("/home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? externalLoginError = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrWhiteSpace(externalLoginError))
            {
                TempData["Error"] = externalLoginError;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Đăng nhập thành công!";
                    return Redirect("/home");
                }

                ModelState.AddModelError(string.Empty, "Tên tài khoản hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) && returnUrl != "/" ? returnUrl : "/home";

            if (remoteError != null)
            {
                TempData["Error"] = $"Lỗi từ dịch vụ ngoài: {remoteError}";
                return RedirectToAction("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "Lỗi khi lấy thông tin đăng nhập Google.";
                return RedirectToAction("Login");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                TempData["Success"] = "Đăng nhập Google thành công!";
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                TempData["Error"] = "Tài khoản đang bị khóa.";
                return RedirectToAction("AccessDenied");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                TempData["Error"] = "Không thể lấy Email từ tài khoản Google của bạn.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email.Split('@')[0] + "_" + Guid.NewGuid().ToString("N")[..4],
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Không thể tạo tài khoản từ liên kết Google.";
                    return RedirectToAction("Login");
                }

                await _userManager.AddToRoleAsync(user, "STUDENT");
            }
            else if (!await _userManager.IsInRoleAsync(user, "STUDENT"))
            {
                await _userManager.AddToRoleAsync(user, "STUDENT");
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                TempData["Success"] = "Đăng ký và đăng nhập bằng Google thành công!";
                return LocalRedirect(returnUrl);
            }

            TempData["Error"] = "Liên kết tài khoản Google thất bại.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
