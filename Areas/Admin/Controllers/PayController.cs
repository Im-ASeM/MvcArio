using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Migrations;

[Area("Admin")]
[Authorize]
public class PayController : Controller
{
    private readonly Context db;
    public PayController(Context _db)
    {
        db = _db;
    }
    public IActionResult UserReports(int UserId, int page = 1, int pageSize = 20, int? Type = null)
    {
        var user = db.Users.Include(x => x.Transactions).FirstOrDefault(x => x.Id == UserId);
        if (user == null)
        {
            TempData["Error"] = "کاربر پیدا نشد";
            return RedirectToAction("index", "home");
        }

        var q = user.Transactions!.OrderByDescending(x => x.Id).ToList();
        if (Type == 0) q = q.Where(x => x.Type == TransactionType.Deposit).ToList();
        else if (Type == 1) q = q.Where(x => x.Type == TransactionType.Withdrawal).ToList();
        var totalTransactions = q.Count;
        var pagedTransactions = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var pagination = new Pagination<Transaction>(pagedTransactions, totalTransactions, pageSize, page);
        ViewBag.pagination = pagination;

        ViewBag.walet = user.Balance;

        return View();
    }
    public IActionResult Index()
    {
        var q = db.DepositRequests.Include(x => x.Card).Include(x => x.User).OrderByDescending(x => x.Id).ToList();
        var pagination = new Pagination<DepositRequest>(q, q.Count, q.Count, 1);
        ViewBag.pagination = pagination;
        return View();
    }
    public IActionResult Withdraw()
    {
        var q = db.WithdrawalRequests.Include(x => x.User).OrderByDescending(x => x.Id).ToList();
        var pagination = new Pagination<WithdrawalRequest>(q, q.Count, q.Count, 1);
        ViewBag.pagination = pagination;
        return View();
    }
    public IActionResult Cards()
    {
        var q = db.Cards.OrderByDescending(x => x.Id).ToList();
        var pagination = new Pagination<Cards>(q, q.Count, q.Count, 1);
        ViewBag.pagination = pagination;
        return View();
    }
    public IActionResult CardStatus(int Id)
    {
        var q = db.Cards.Find(Id);
        if (q != null)
        {
            q.IsActive = !q.IsActive;
            db.Cards.Update(q);
            db.SaveChanges();
        }
        return RedirectToAction("Cards");
    }
    public IActionResult NewCard()
    {
        return View();
    }

    [HttpPost]
    public IActionResult WithdrawReview(int Id, string msg)
    {
        var q = db.WithdrawalRequests.Include(x => x.Transaction)!.FirstOrDefault(x => x.Id == Id);
        q!.IsValid = true;
        q.Description = msg;
        q.CheckTime = DateTime.Now;
        q.Transaction!.Details = q.Description;
        db.WithdrawalRequests.Update(q);
        db.SaveChanges();
        return RedirectToAction("Withdraw");
    }

    [HttpPost]
    public IActionResult DepositReview(int Id, string msg, bool IsValid)
    {
        var q = db.DepositRequests.Find(Id)!;
        q.IsValid = IsValid;
        q.Description = msg;
        q.CheckTime = DateTime.Now;
        db.DepositRequests.Update(q);
        db.SaveChanges();

        NewTransaction transaction = new NewTransaction
        {
            Amount = Convert.ToInt32(q.Amount),
            Description = "افزایش موجودی از طریق کارت به کارت",
            Details = q.Description,
            Type = TransactionType.Deposit,
            UserId = q.UserId
        };

        var result = AddTransaction(transaction);
        if (result is OkResult)
        {
            return RedirectToAction("Index");
        }
        else
        {
            var badRequestResult = result as BadRequestObjectResult;
            TempData["Error"] = badRequestResult?.Value?.ToString() ?? "خطایی رخ داده است.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult AddCard(NewCard newCard)
    {
        db.Cards.Add(new Cards
        {
            CardBank = newCard.CardBank,
            CardName = newCard.CardName,
            CardNumber = newCard.CardNumber,
            IsActive = true
        });
        db.SaveChanges();
        return RedirectToAction("Cards");
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
}