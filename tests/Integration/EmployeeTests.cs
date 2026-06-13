using CafeApp.Data;
using CafeApp.Models;
using CafeApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Tests.Integration;

public class EmployeeTests : IDisposable
{
    private readonly DatabaseFixture _fixture = new();
    private readonly AppDbContext _db;

    public EmployeeTests()
    {
        _db = _fixture.DbContext;
    }

    public void Dispose() => _fixture.Dispose();

    [Test]
    public async Task CreateEmployee_StoresRecord()
    {
        var employee = new Employee
        {
            EntraObjectId = "test-123",
            DisplayName = "Test User",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var retrieved = await _db.Employees
            .FirstOrDefaultAsync(e => e.EntraObjectId == "test-123");

        await Assert.That(retrieved).IsNotNull();
        await Assert.That(retrieved!.DisplayName).IsEqualTo("Test User");
        await Assert.That(retrieved.Email).IsEqualTo("test@example.com");
    }

    [Test]
    public async Task UpdateEmployee_ModifiesExistingRecord()
    {
        var employee = new Employee
        {
            EntraObjectId = "test-456",
            DisplayName = "Original Name",
            Email = "original@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var retrieved = await _db.Employees
            .FirstOrDefaultAsync(e => e.EntraObjectId == "test-456");

        retrieved!.DisplayName = "Updated Name";
        retrieved.Email = "updated@example.com";
        await _db.SaveChangesAsync();

        var final = await _db.Employees
            .FirstOrDefaultAsync(e => e.EntraObjectId == "test-456");

        await Assert.That(final!.DisplayName).IsEqualTo("Updated Name");
        await Assert.That(final.Email).IsEqualTo("updated@example.com");
    }
}
