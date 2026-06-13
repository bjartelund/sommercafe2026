using CafeApp.Data;
using CafeApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafeApp.Functions;

public class EmployeeFunctions(AppDbContext db)
{
    [Authorize]
    [Function("PostEmployeeMe")]
    public async Task<IActionResult> PostEmployeeMe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employees/me")] HttpRequestData req,
        ClaimsPrincipal claimsPrincipal)
    {
        string objectId = null;
        string displayName = null;
        string email = null;

        // Try to extract from claims (real auth)
        if (claimsPrincipal != null)
        {
            objectId = claimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? claimsPrincipal.FindFirst("oid")?.Value;
            displayName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                ?? claimsPrincipal.FindFirst("name")?.Value;
            email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                ?? claimsPrincipal.FindFirst("email")?.Value;
        }

        // Fallback for local development (SWA mock auth, no claims)
        if (string.IsNullOrEmpty(objectId) || string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(email))
        {
            objectId = "test-user-" + Guid.NewGuid().ToString().Substring(0, 8);
            displayName = "Test User";
            email = "test@cafe.local";
        }

        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.EntraObjectId == objectId);

        if (employee == null)
        {
            employee = new Employee
            {
                EntraObjectId = objectId,
                DisplayName = displayName,
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Employees.Add(employee);
        }
        else
        {
            employee.DisplayName = displayName;
            employee.Email = email;
        }

        await db.SaveChangesAsync();
        return new OkObjectResult(employee);
    }
}
