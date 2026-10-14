using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicLeaveManager.Pages;

public class IndexModel : PageModel
{
    private const string UserKey = "CampusLeave.User";
    private const string RequestsKey = "CampusLeave.Requests";
    private const string RequestTokenKey = "CampusLeave.LastToken";
    private const string RememberCookie = "CampusLeave.Remember";
    private static readonly string[] LeaveTypes = ["Casual leave", "Sick leave", "Earned leave", "Academic leave"];

    [BindProperty] public LoginInput Login { get; set; } = new();
    [BindProperty] public LeaveInput Leave { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Month { get; set; }
    [BindProperty(SupportsGet = true)] public string? SelectedDate { get; set; }

    public string? UserName { get; private set; }
    public string? RememberedUser { get; private set; }
    public string MonthTitle { get; private set; } = "";
    public DateOnly MonthDate { get; private set; }
    public IReadOnlyList<CalendarDay> CalendarDays { get; private set; } = [];
    public IReadOnlyList<LeaveRequest> Requests { get; private set; } = [];
    public IReadOnlyList<CampusEvent> Events { get; private set; } = [];
    public string? Notice { get; private set; }
    public string RequestToken { get; private set; } = "";

    public void OnGet(string? notice = null)
    {
        LoadPage(notice);
        if (DateOnly.TryParseExact(SelectedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var selected))
            Leave.StartDate = selected.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (string.IsNullOrWhiteSpace(Login.UserName))
            Login.UserName = RememberedUser ?? "";
    }

    public IActionResult OnPostLogin()
    {
        ModelState.Clear();
        if (!TryValidateModel(Login, nameof(Login)))
        {
            LoadPage();
            return Page();
        }

        // Demonstration accounts only; replace this in a deployed system with a proper identity provider.
        var valid = (Login.UserName.Equals("student", StringComparison.OrdinalIgnoreCase) && Login.Password == "Student@123")
                 || (Login.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase) && Login.Password == "Admin@123");
        if (!valid)
        {
            ModelState.AddModelError("Login.Password", "That username and password do not match.");
            LoadPage();
            return Page();
        }

        var user = Login.UserName.Trim();
        HttpContext.Session.SetString(UserKey, user);
        if (Login.RememberMe)
        {
            Response.Cookies.Append(RememberCookie, user, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(14), HttpOnly = true,
                SameSite = SameSiteMode.Lax, IsEssential = true, Secure = Request.IsHttps
            });
        }
        else
        {
            Response.Cookies.Delete(RememberCookie, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps });
        }
        return RedirectToPage("/Index");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(RememberCookie, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps });
        return RedirectToPage("/Index");
    }

    public IActionResult OnPostSubmitLeave()
    {
        if (HttpContext.Session.GetString(UserKey) is null)
            return RedirectToPage("/Index");

        ModelState.Clear();
        TryValidateModel(Leave, nameof(Leave));

        if (!LeaveTypes.Contains(Leave.Type, StringComparer.Ordinal))
            ModelState.AddModelError("Leave.Type", "Choose one of the listed leave categories.");
        if (!DateOnly.TryParseExact(Leave.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            ModelState.AddModelError("Leave.StartDate", "Choose a valid start date.");
        if (!DateOnly.TryParseExact(Leave.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            ModelState.AddModelError("Leave.EndDate", "Choose a valid end date.");
        if (ModelState.IsValid && end < start)
            ModelState.AddModelError("Leave.EndDate", "The end date must be on or after the start date.");
        if (ModelState.IsValid && start < DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("Leave.StartDate", "Leave requests must start today or later.");

        if (!ModelState.IsValid)
        {
            LoadPage();
            return Page();
        }

        var requests = GetRequests().ToList();
        var token = Request.Form["RequestToken"].ToString();
        var lastToken = HttpContext.Session.GetString(RequestTokenKey);
        if (!string.IsNullOrWhiteSpace(token) && token == lastToken)
        {
            requests.Insert(0, new LeaveRequest(Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(),
                HttpContext.Session.GetString(UserKey)!, Leave.Type, start, end, Leave.Reason.Trim(), "Pending", DateTimeOffset.Now));
            HttpContext.Session.SetString(RequestsKey, JsonSerializer.Serialize(requests));
            HttpContext.Session.SetString(RequestTokenKey, Guid.NewGuid().ToString("N"));
        }

        return RedirectToPage("/Index", new { month = start.ToString("yyyy-MM", CultureInfo.InvariantCulture), notice = "Your leave request is saved for review." });
    }

    private void LoadPage(string? notice = null)
    {
        UserName = HttpContext.Session.GetString(UserKey);
        RememberedUser = Request.Cookies[RememberCookie];
        Notice = notice;
        Requests = GetRequests();
        RequestToken = HttpContext.Session.GetString(RequestTokenKey) ?? Guid.NewGuid().ToString("N");
        HttpContext.Session.SetString(RequestTokenKey, RequestToken);
        var today = DateOnly.FromDateTime(DateTime.Today);
        MonthDate = DateOnly.TryParseExact(Month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? new DateOnly(parsed.Year, parsed.Month, 1) : new DateOnly(today.Year, today.Month, 1);
        MonthTitle = MonthDate.ToDateTime(TimeOnly.MinValue).ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        Events = GetEvents(MonthDate.Year).Where(e => e.Date.Month == MonthDate.Month).ToArray();
        var first = new DateOnly(MonthDate.Year, MonthDate.Month, 1);
        var offset = ((int)first.DayOfWeek + 6) % 7;
        var gridStart = first.AddDays(-offset);
        CalendarDays = Enumerable.Range(0, 42).Select(i =>
        {
            var date = gridStart.AddDays(i);
            return new CalendarDay(date, date.Month == MonthDate.Month, date == today,
                date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) == SelectedDate,
                Events.Any(e => e.Date == date), Requests.Any(r => date >= r.StartDate && date <= r.EndDate && r.User == UserName));
        }).ToArray();
        if (string.IsNullOrEmpty(Leave.StartDate)) Leave.StartDate = SelectedDate ?? today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (string.IsNullOrEmpty(Leave.EndDate)) Leave.EndDate = Leave.StartDate;
    }

    private IReadOnlyList<LeaveRequest> GetRequests()
    {
        var stored = HttpContext.Session.GetString(RequestsKey);
        return string.IsNullOrWhiteSpace(stored) ? [] : JsonSerializer.Deserialize<List<LeaveRequest>>(stored) ?? [];
    }

    private static IReadOnlyList<CampusEvent> GetEvents(int year) =>
    [
        new(new DateOnly(year, 1, 26), "Republic Day", "Holiday", "National holiday"),
        new(new DateOnly(year, 3, 8), "Mid-semester review", "Academic", "Faculty progress review"),
        new(new DateOnly(year, 5, 1), "Labour Day", "Holiday", "Campus holiday"),
        new(new DateOnly(year, 6, 12), "Semester begins", "Academic", "First day of the new semester"),
        new(new DateOnly(year, 8, 15), "Independence Day", "Holiday", "National holiday"),
        new(new DateOnly(year, 9, 5), "Teacher's Day", "Academic", "Campus programme"),
        new(new DateOnly(year, 10, 2), "Gandhi Jayanti", "Holiday", "National holiday"),
        new(new DateOnly(year, 11, 14), "Project showcase", "Academic", "Student project presentations"),
        new(new DateOnly(year, 12, 25), "Winter break", "Holiday", "Campus holiday")
    ];

    public sealed class LoginInput
    {
        [Required, StringLength(40)] public string UserName { get; set; } = "";
        [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    public sealed class LeaveInput
    {
        [Required] public string Type { get; set; } = "Casual leave";
        [Required, StringLength(500, MinimumLength = 8)] public string Reason { get; set; } = "";
        [Required] public string StartDate { get; set; } = "";
        [Required] public string EndDate { get; set; } = "";
    }

    public record CalendarDay(DateOnly Date, bool InMonth, bool IsToday, bool IsSelected, bool HasEvent, bool HasLeave);
    public record CampusEvent(DateOnly Date, string Title, string Category, string Detail);
    public record LeaveRequest(string Id, string User, string Type, DateOnly StartDate, DateOnly EndDate, string Reason, string Status, DateTimeOffset SubmittedAt);
}
