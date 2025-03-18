using System.Globalization;
using System.Security.Claims;
using Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ClientController : Controller
{
    private readonly Context db;
    public ClientController(Context _db)
    {
        db = _db;
    }

    [Authorize]
    public IActionResult Index()
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        return View();
    }

    public IActionResult Rules()
    {
        return View();
    }

    [Authorize]
    public IActionResult profile()
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        var UserId = Convert.ToInt32(User.FindFirstValue("Id"));
        var user = db.Users.Find(UserId);
        ViewBag.User = user;
        return View();
    }

    public IActionResult ContactUs()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public IActionResult profile(User user)
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        var UserId = Convert.ToInt32(User.FindFirstValue("Id"));
        var result = db.Users.Find(UserId)!;
        result.Name = user.Name;
        result.Password = user.Password;
        db.Users.Update(result);
        db.SaveChanges();
        TempData["Success"] = "بروزرسانی پروفایل با موفقیت انجام شد.";
        return RedirectToAction("index");
    }



    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (IsAdmin())
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else
            {
                return RedirectToAction("index");
            }
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var user = db.Users.FirstOrDefault(u => u.UserName == loginRequest.Username && u.Password == loginRequest.Password);
        if (user != null)
        {
            if (user.Status)
            {
                var claims = new List<Claim>
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("name", user.Name),
                    new Claim("Username",user.UserName),
                    new Claim("Role","Client")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1) // زمان انقضا
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["success"] = "سلام ! به پنل مدیریت خوش آمدید.";
                return RedirectToAction("index");
            }
            else
            {
                TempData["Error"] = "ورود شما توسط مدیریت محدود شده . لطفا با پشتیبانی تماس بگیرید.";
                return View();
            }
        }
        else
        {
            TempData["Error"] = "نام کاربری یا رمز عبور اشتباه است";
            return View();
        }
    }

    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (db.Users.Any(u => u.UserName == user.UserName))
        {
            TempData["Error"] = "نام کاربری قبلاً استفاده شده است.";
            return View();
        }

        if (db.Users.Any(u => u.PhoneNumber == user.PhoneNumber))
        {
            TempData["Error"] = "شماره تلفن قبلاً ثبت شده است.";
            return View();
        }

        db.Users.Add(user);
        db.SaveChanges();
        TempData["success"] = "کاربر با موفقیت ثبت شد";
        return RedirectToAction("Login");
    }

    [Authorize]
    public IActionResult AddMoney()
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        List<IncomingCard> results = new List<IncomingCard>();
        List<Cards> cards = db.Cards.Where(x => x.IsActive).ToList();
        foreach (Cards card in cards)
        {
            results.Add(new IncomingCard()
            {
                CardBank = card.CardBank,
                CardName = card.CardName,
                CardNumber = card.CardNumber,
                Id = card.Id
            });
        }
        ViewBag.Cards = results;
        return View();
    }

    [Authorize]
    public IActionResult CartToCart(int Id)
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        Cards? cards = db.Cards.Find(Id);
        ViewBag.Card = cards;
        if (cards != null)
        {
            return cards.IsActive ? View() : RedirectToAction("AddMoney");
        }
        return RedirectToAction("AddMoney");
    }

    [Authorize]
    [HttpPost]
    public IActionResult CartToCart(c2cRequest c2c)
    {
        if (IsAdmin())
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
        int UserId = Convert.ToInt32(User.FindFirstValue("id"));
        if (db.DepositRequests.Any(x => x.UserId == UserId && x.IsValid == null && x.CreateDateTime.AddHours(5) > DateTime.Now))
        {
            TempData["Error"] = "شما در ساعات گذشته یک درخواست ارسال کردید. لطفا صبر کنید";
            return RedirectToAction("index");
        }
        DepositRequest request = new DepositRequest
        {
            Amount = c2c.Price,
            CardId = c2c.CardId,
            ClientCardNumber = c2c.ClientCardNumber,
            CheckTime = DateTime.Now,
            CreateDateTime = DateTime.Now,
            IsValid = null,
            UserId = Convert.ToInt32(User.FindFirstValue("id"))
        };
        db.DepositRequests.Add(request);
        db.SaveChanges();
        TempData["Success"] = "ثبت درخواست با موفقیت انجام شد.";
        return RedirectToAction("index");
    }

    public IActionResult Trace()
    {
        int UserId = Convert.ToInt32(User.FindFirstValue("id"));

        User user = db.Users.Include(x => x.Transactions).FirstOrDefault(x => x.Id == UserId)!;
        List<TraceHistory> histories = new List<TraceHistory>();
        PersianCalendar pc = new PersianCalendar();
        if (user.Transactions != null)
        {
            foreach (Transaction item in user.Transactions.OrderByDescending(x => x.Id))
            {
                histories.Add(new TraceHistory
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    Date = $"{pc.GetYear(item.CreateDateTime)}/{pc.GetMonth(item.CreateDateTime)}/{pc.GetDayOfMonth(item.CreateDateTime)}",
                    Title = item.Description,
                    Description = item.Details,
                    type = item.Type
                });
            }
        }
        ViewBag.Traces = new Traces
        {
            Balance = user.Balance,
            TraceHistories = histories
        };
        return View();
    }

    public IActionResult Withdrawal()
    {
        int UserId = Convert.ToInt32(User.FindFirstValue("id"));
        User user = db.Users.Include(x => x.Transactions).FirstOrDefault(x => x.Id == UserId)!;
        user.CheckBalance();
        ViewBag.Balance = user.Balance;
        return View();
    }

    [HttpPost]
    public IActionResult Withdrawal(c2cRequest c2c)
    {
        int UserId = Convert.ToInt32(User.FindFirstValue("id"));
        int AmountNum = Convert.ToInt32(c2c.Price);

        User user = db.Users.Include(x => x.Transactions).FirstOrDefault(x => x.Id == UserId)!;
        user.CheckBalance();

        if (user.Balance < AmountNum)
        {
            db.Users.Update(user);
            db.SaveChanges();
            TempData["Error"] = "مبلغ درخواستی بیشتر از کیف پول میباشد.";
            return RedirectToAction("Index");
        }
        else if (AmountNum < 100000)
        {
            db.Users.Update(user);
            db.SaveChanges();
            TempData["Error"] = "مبلغ درخواستی کمتر از صد هزار تومان میباشد.";
            return RedirectToAction("index");
        }

        NewTransaction transaction = new NewTransaction
        {
            Amount = AmountNum,
            Description = "کاهش موجودی به موجب برداشت وجه",
            Details = $"کاربر {user.Name} با نام کاربری {user.UserName} در تاریخ {PersianDate.ToPersianDateString(DateTime.Now)} ساعت {(DateTime.Now.Hour.ToString().Count() == 1 ? $"0{DateTime.Now.Hour}" : DateTime.Now.Hour)}:{(DateTime.Now.Minute.ToString().Count() == 1 ? $"0{DateTime.Now.Minute}" : DateTime.Now.Minute)} مبلغ {AmountNum} تومان از {user.Balance} تومان موجودی خویش را درخواست کرد. مبلغ ذکر شده به حسابی با شماره {c2c.ClientCardNumber} واریز خواهد شد. ",
            Type = TransactionType.Withdrawal,
            UserId = UserId
        };

        var result = AddTransaction(transaction);

        if (result is not OkResult)
        {
            var badRequestResult = result as BadRequestObjectResult;
            TempData["Error"] = badRequestResult?.Value?.ToString() ?? "خطایی رخ داده است.";
            return RedirectToAction("Index");
        }
        WithdrawalRequest withdrawalRequest = new WithdrawalRequest
        {
            Amount = c2c.Price,
            ClientCardNumber = c2c.ClientCardNumber,
            CreateDateTime = DateTime.Now,
            CheckTime = DateTime.Now,
            UserId = UserId,
            IsValid = false,
            Description = transaction.Details,
            TransactionId = user.Transactions!.OrderByDescending(x => x.Id).FirstOrDefault()!.Id
        };
        db.WithdrawalRequests.Add(withdrawalRequest);
        db.SaveChanges();
        TempData["Success"] = "درخواست شما با موفقیت ثبت شد .";
        return RedirectToAction("index");
    }
    private IActionResult AddTransaction(NewTransaction Transaction)
    {
        try
        {
            User user = db.Users.Include(x => x.Transactions).FirstOrDefault(x => x.Id == Transaction.UserId)!;

            if (Transaction.Type == TransactionType.Deposit)
            {
                user.Deposit(db, Transaction.Amount, Transaction.Description, Transaction.Details);
            }

            else
            {
                user.CheckBalance();
                user.Withdraw(db, Transaction.Amount, Transaction.Description, Transaction.Details);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    private bool IsAdmin()
    {
        string role = User.FindFirstValue("Role")!;
        return role != "Client";
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        if (IsAdmin())
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Home", new { area = "Admin" });
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Client");
    }
}