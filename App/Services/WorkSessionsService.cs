using System.Text.Json.Serialization;
using CafeApp.Data;
using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Services;

public class WorkSessionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("employeeId")]
    public int EmployeeId { get; set; }

    [JsonPropertyName("sessionDate")]
    public DateTime SessionDate { get; set; }

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("employee")]
    public EmployeeResponse? Employee { get; set; }
}

public class WorkSessionsService(IDbContextFactory<AppDbContext> dbFactory, EmployeeService employeeService)
{
    public async Task<List<WorkSessionResponse>> GetWorkSessionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow.AddDays(1);

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.WorkSessions
            .Where(ws => ws.SessionDate >= fromDate && ws.SessionDate <= toDate)
            .Include(ws => ws.Employee)
            .OrderByDescending(ws => ws.SessionDate)
            .ThenByDescending(ws => ws.StartTime)
            .Select(ws => Map(ws))
            .ToListAsync();
    }

    public async Task<WorkSessionResponse?> CreateWorkSessionAsync(DateTime sessionDate, TimeSpan startTime, TimeSpan endTime, string? notes = null)
    {
        if (endTime <= startTime)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var employeeId = await employeeService.GetCurrentEmployeeIdAsync();
        if (employeeId <= 0)
            return null;

        var session = new WorkSession
        {
            EmployeeId = employeeId,
            SessionDate = sessionDate.Date,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = (int)(endTime - startTime).TotalMinutes,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        db.WorkSessions.Add(session);
        await db.SaveChangesAsync();

        await db.Entry(session).Reference(s => s.Employee).LoadAsync();
        return Map(session);
    }

    public async Task<WorkSessionResponse?> UpdateWorkSessionAsync(int id, DateTime sessionDate, TimeSpan startTime, TimeSpan endTime, string? notes = null)
    {
        if (endTime <= startTime)
            return null;

        await using var db = await dbFactory.CreateDbContextAsync();
        var session = await db.WorkSessions.Include(ws => ws.Employee).FirstOrDefaultAsync(ws => ws.Id == id);
        if (session == null)
            return null;

        session.SessionDate = sessionDate.Date;
        session.StartTime = startTime;
        session.EndTime = endTime;
        session.DurationMinutes = (int)(endTime - startTime).TotalMinutes;
        session.Notes = notes;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Map(session);
    }

    public async Task<bool> DeleteWorkSessionAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var session = await db.WorkSessions.FindAsync(id);
        if (session == null)
            return false;

        db.WorkSessions.Remove(session);
        await db.SaveChangesAsync();
        return true;
    }

    private static WorkSessionResponse Map(WorkSession session)
    {
        return new WorkSessionResponse
        {
            Id = session.Id,
            EmployeeId = session.EmployeeId,
            SessionDate = session.SessionDate,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            DurationMinutes = session.DurationMinutes,
            Notes = session.Notes,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            Employee = session.Employee == null
                ? null
                : new EmployeeResponse
                {
                    Id = session.Employee.Id,
                    EntraObjectId = session.Employee.EntraObjectId,
                    DisplayName = session.Employee.DisplayName,
                    Email = session.Employee.Email,
                    IsActive = session.Employee.IsActive,
                    CreatedAt = session.Employee.CreatedAt
                }
        };
    }
}