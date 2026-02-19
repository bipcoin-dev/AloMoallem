using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using AloMoallem.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AloMoallem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _db;
    private readonly IEmailSender _emailSender;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext db, IEmailSender emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var profs = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ViewBag.Professions = profs;
        var govs = await _db.Governorates.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        ViewBag.Governorates = govs;
        return View(new RegisterVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm, IFormFile? avatar)
    {
        var profs = await _db.Professions.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
        ViewBag.Professions = profs;
        var govs = await _db.Governorates.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        ViewBag.Governorates = govs;

        if (vm.AccountType == "Artisan")
        {
            if (vm.ProfessionIds is null || vm.ProfessionIds.Count == 0)
                ModelState.AddModelError(nameof(vm.ProfessionIds), "اختر مهنة واحدة على الأقل");
            if (string.IsNullOrWhiteSpace(vm.DisplayName))
                ModelState.AddModelError(nameof(vm.DisplayName), "الاسم المعروض مطلوب");
        }

        if (!ModelState.IsValid) return View(vm);

        var user = new AppUser { UserName = vm.Email, Email = vm.Email, AccountType = vm.AccountType };
        var result = await _userManager.CreateAsync(user, vm.Password);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, vm.AccountType);

        // زبون: إنشاء ملف زبون
        if (vm.AccountType == "Customer")
        {
            _db.CustomerProfiles.Add(new CustomerProfile
            {
                UserId = user.Id,
                FullName = string.IsNullOrWhiteSpace(vm.CustomerFullName) ? (vm.Email.Split('@')[0]) : vm.CustomerFullName,
                GovernorateId = vm.GovernorateId,
                NeighborhoodId = vm.NeighborhoodId
            });
            await _db.SaveChangesAsync();
        }

        if (vm.AccountType == "Artisan")
        {
            var ids = (vm.ProfessionIds ?? new()).Distinct().ToList();
            var primaryId = ids.First();
            var primaryProf = await _db.Professions.FirstAsync(p => p.Id == primaryId);

            var profile = new ArtisanProfile
            {
                UserId = user.Id,
                ProfessionId = primaryProf.Id,
                DisplayName = vm.DisplayName,
                City = "",
                GovernorateId = vm.GovernorateId,
                NeighborhoodId = vm.NeighborhoodId,
                About = vm.About,
                PhoneNumberPublic = vm.PhoneNumberPublic,
                PhotoUrl = "/img/default-avatar.svg",
                IsVerified = false,
                Rating = 5.0,
                CompletedJobs = 0
            };

            // Avatar upload (اختياري أثناء التسجيل)
            if (avatar is not null && avatar.Length > 0)
            {
                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var uploads = Path.Combine(env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(uploads);
                var ext = Path.GetExtension(avatar.FileName);
                var fileName = $"{Guid.NewGuid():N}{ext}";
                var path = Path.Combine(uploads, fileName);
                await using var stream = System.IO.File.Create(path);
                await avatar.CopyToAsync(stream);
                profile.PhotoUrl = $"/uploads/avatars/{fileName}";
            }

            _db.ArtisanProfiles.Add(profile);
            await _db.SaveChangesAsync();

            // ربط المهن (بما فيها الأساسية)
            foreach (var id in ids)
            {
                _db.ArtisanProfileProfessions.Add(new ArtisanProfileProfession
                {
                    ArtisanProfileId = profile.Id,
                    ProfessionId = id
                });
            }
            await _db.SaveChangesAsync();
        }

        await SendEmailConfirmationAsync(user);
        return View("EmailSent");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is null)
        {
            ModelState.AddModelError("", "بيانات الدخول غير صحيحة");
            return View(vm);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            ModelState.AddModelError("", "لازم تأكيد الإيميل قبل تسجيل الدخول. تفقد مجلد App_Data/emails عندك (نسخة تطوير).");
            return View(vm);
        }

        var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "بيانات الدخول غير صحيحة");
            return View(vm);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SendEmailConfirmationAsync(AppUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, protocol: Request.Scheme);

        var body = $@"
<p>مرحباً،</p>
<p>لتأكيد حسابك على منصة مُعلّم اضغط الرابط التالي:</p>
<p><a href='{link}'>تأكيد الحساب</a></p>
<p>إذا ما كنت أنت، تجاهل الرسالة.</p>";

        await _emailSender.SendAsync(user.Email!, "تأكيد حساب منصة مُعلّم", body);
    }

    private async Task SendPasswordResetAsync(AppUser user)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var link = Url.Action("ResetPassword", "Account", new { email = user.Email, token }, protocol: Request.Scheme);

        var body = $@"
<p>مرحباً،</p>
<p>لإعادة تعيين كلمة المرور اضغط الرابط التالي:</p>
<p><a href='{link}'>إعادة تعيين كلمة المرور</a></p>
<p>إذا ما طلبت ذلك، تجاهل الرسالة.</p>";

        await _emailSender.SendAsync(user.Email!, "إعادة تعيين كلمة المرور — منصة مُعلّم", body);
    }

}

public class RegisterVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    public string AccountType { get; set; } = "Customer";

    // Location (للزبون والحرفي)
    [Required]
    public int GovernorateId { get; set; }

    [Required]
    public int NeighborhoodId { get; set; }

    // Customer
    public string CustomerFullName { get; set; } = "";

    // Artisan
    // artisan: اختيار أكثر من مهنة
    public List<int> ProfessionIds { get; set; } = new();
    public string DisplayName { get; set; } = "";
    public string About { get; set; } = "";
    public string PhoneNumberPublic { get; set; } = "";

    // UI helper
    public string City { get; set; } = "";
}

public class ForgotPasswordVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";
}

public class ResetPasswordVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Token { get; set; } = "";

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = "";

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
