using CafeApp.Data;
using CafeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CafeApp.Functions;

public class WorkSessionsFunctions(AppDbContext db)
{
    [Authorize]
    [Function("CreateWorkSession")]
    public async Task<IActionResult> CreateWorkSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "work-sessions")] HttpRequestData req,
        ClaimsPrincipal claimsPrincipal)
    {
        var objectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? claimsPrincipal.FindFirst("oid")?.Value;

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EntraObjectId == objectId);
        if (employee == null)
            return new BadRequestObjectResult("Employee not found");

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        var startTimeStr = body?.RootElement.GetProperty("startTime").GetString() ?? "";
        var endTimeStr = body?.RootElement.GetProperty("endTime").GetString() ?? "";

        if (!TimeSpan.TryParse(startTimeStr, out var startTime) || !TimeSpan.TryParse(endTimeStr, out var endTime))
            return new BadRequestObjectResult("Invalid time format");

        if (endTime <= startTime)
            return new BadRequestObjectResult("End time must be after start time");

        var duration = (int)(endTime - startTime).TotalMinutes;
        var sessionDate = DateTime.UtcNow.Date;
        if (body?.RootElement.TryGetProperty("sessionDate", out var dateElem) ?? false)
            DateTime.TryParse(dateElem.GetString() ?? "", out sessionDate);

        var session = new WorkSession
        {
            EmployeeId = employee.Id,
            SessionDate = sessionDate,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = duration,
            Notes = body?.RootElement.TryGetProperty("notes", out var notesElem) ?? false ? notesElem.GetString() : null,
            CreatedAt = DateTime.UtcNow
        };

        db.WorkSessions.Add(session);
        await db.SaveChangesAsync();

        return new CreatedResult($"/api/work-sessions/{session.Id}", session);
    }

    [Authorize]
    [Function("GetWorkSessions")]
    public async Task<IActionResult> GetWorkSessions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "work-sessions")] HttpRequestData req)
    {
        var fromDate = DateTime.TryParse(req.Query["from"] ?? "", out var f) ? f : DateTime.UtcNow.AddMonths(-1);
        var toDate = DateTime.TryParse(req.Query["to"] ?? "", out var t) ? t : DateTime.UtcNow;

        var sessions = await db.WorkSessions
            .Where(s => s.SessionDate >= fromDate && s.SessionDate <= toDate)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();

        return new OkObjectResult(sessions);
    }

    [Authorize]
    [Function("UpdateWorkSession")]
    public async Task<IActionResult> UpdateWorkSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "work-sessions/{id}")] HttpRequestData req,
        int id)
    {
        var session = await db.WorkSessions.FindAsync(id);
        if (session == null)
            return new NotFoundResult();

        JsonDocument? body = null;
        try { body = await JsonDocument.ParseAsync(req.Body); } catch { }

        if (body?.RootElement.TryGetProperty("endTime", out var endTimeElem) ?? false)
        {
            if (TimeSpan.TryParse(endTimeElem.GetString() ?? "", out var endTime))
            {
                session.EndTime = endTime;
                if (session.EndTime > session.StartTime)
                {
                    session.DurationMinutes = (int)(session.EndTime - session.StartTime).TotalMinutes;
                }
            }
        }

        session.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new OkObjectResult(session);
    }

    [Authorize]
    [Function("DeleteWorkSession")]
    public async Task<IActionResult> DeleteWorkSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "work-sessions/{id}")] HttpRequestData req,
        int id)
    {
        var session = await db.WorkSessions.FindAsync(id);
        if (session == null)
            return new NotFoundResult();

        db.WorkSessions.Remove(session);
        await db.SaveChangesAsync();

        return new OkResult();
    }
}
