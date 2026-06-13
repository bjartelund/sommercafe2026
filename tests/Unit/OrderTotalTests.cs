namespace CafeApp.Tests.Unit;

public class OrderTotalTests
{
    [Test]
    public async Task CalculateLineTotal_MultiplyUnitPriceByQuantity()
    {
        decimal unitPrice = 10m;
        int quantity = 2;
        decimal expectedTotal = 20m;
        decimal actual = unitPrice * quantity;
        await Assert.That(actual).IsEqualTo(expectedTotal);
    }

    [Test]
    public async Task CalculateOrderTotal_SumMultipleLines()
    {
        var lines = new[]
        {
            new { UnitPrice = 2.50m, Quantity = 2, LineTotal = 5.00m },
            new { UnitPrice = 3.00m, Quantity = 1, LineTotal = 3.00m },
            new { UnitPrice = 1.50m, Quantity = 4, LineTotal = 6.00m }
        };

        decimal total = lines.Sum(l => l.UnitPrice * l.Quantity);
        await Assert.That(total).IsEqualTo(14.00m);
    }

    [Test]
    public async Task CalculateOrderTotal_SingleLine()
    {
        decimal total = 2.50m * 1;
        await Assert.That(total).IsEqualTo(2.50m);
    }
}
