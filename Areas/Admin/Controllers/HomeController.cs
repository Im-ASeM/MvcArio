using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
public class HomeController : Controller
{
    private readonly Context db;
    public HomeController(Context _db)
    {
        db = _db;
    }

    [Authorize]
    public IActionResult Index(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        if (!IsAdmin())
        {
            return Redirect("/");
        }
        var users = db.Users.ToList();
        if (!string.IsNullOrEmpty(searchTerm))
        {
            users = users.Where(u => u.Name.Contains(searchTerm) || u.PhoneNumber.Contains(searchTerm) || u.UserName.Contains(searchTerm)).ToList(); // جستجو بر اساس نام یا ایمیل
        }

        var totalUsers = users.Count();

        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var pagination = new Pagination<User>(pagedUsers, totalUsers, pageSize, page);

        ViewBag.Pagination = pagination;

        return View();
    }
    [Authorize]
    public IActionResult UserStatus(int Id)
    {
        var user = db.Users.Find(Id);
        if (user == null)
        {
            TempData["Error"] = "کاربر یافت نشد.";
            return RedirectToAction("Index");
        }
        user.Status = !user.Status;
        db.Users.Update(user);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (!IsAdmin())
            {
                return Redirect("/");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string Password)
    {
        if (db.Admins.Count() == 0)
        {
            db.Admins.Add(new Admin { CreateDateTime = DateTime.Now, Password = "admin", Username = "admin" });
            db.SaveChanges();
        }
        var Check = db.Admins.FirstOrDefault(x => x.Username.ToLower() == username.Trim().ToLower() && x.Password == Password);
        if (Check == null)
        {
            ViewBag.Error = "نام کاربری یا کلمه عبور نادرست میباشد.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim("id", Check.Id.ToString()),
            new Claim("name", "مدیریت"), // نام
            new Claim("Username",Check.Username),
            new Claim("Role","admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // زمان انقضا
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        TempData["success"] = "سلام ! به پنل مدیریت خوش آمدید.";
        return RedirectToAction("Index");

    }
    private bool IsAdmin()
    {
        string role = User.FindFirstValue("Role")!;
        return role != "Client";
    }
}
