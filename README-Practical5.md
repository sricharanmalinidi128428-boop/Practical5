# Practical 5 — Campus Leave & Academic Calendar

An original ASP.NET Core Razor Pages application for the .NET Technologies Practical 5 requirements: academic calendar, leave requests, session state, and a remember-username cookie.

## Open in Visual Studio

Open `Practical5.sln` from this folder, set `AcademicLeaveManager` as the startup project if prompted, then press **F5**. The launch profile opens the app at `https://localhost:7040` (or `http://localhost:5142`).

## Demonstration accounts

- Student: `student` / `Student@123`
- Admin: `admin` / `Admin@123`

This is a classroom demonstration. Accounts and campus dates are sample data in source code. Leave requests are held in the ASP.NET session and expire when the session expires; there is no database or administrator approval workflow.

## What it demonstrates

- Calendar month navigation, selectable dates, current-day highlight, and academic/holiday/leave indicators.
- Leave categories, date range, reason, server-side validation, and pending request history.
- ASP.NET Core session state for the signed-in user and request list.
- An HttpOnly, SameSite remember-username cookie with a 14-day expiry; the session has a 30-minute idle timeout.
- Responsive page layout and accessible form labels, status messages, and calendar date names.

The sample credentials are intentionally for local coursework only. Do not deploy them as real authentication.
