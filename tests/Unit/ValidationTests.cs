namespace CafeApp.Tests.Unit;

public class ValidationTests
{
    [Test]
    public async Task Placeholder_CompilationTest()
    {
        var value = 1 + 1;
        await Assert.That(value).IsEqualTo(2);
    }
}