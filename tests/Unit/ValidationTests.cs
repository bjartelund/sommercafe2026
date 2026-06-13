namespace CafeApp.Tests.Unit;

public class ValidationTests
{
    [Test]
    public async Task Placeholder_CompilationTest()
    {
        // Placeholder test to verify test project compiles and runs
        await Assert.That(1 + 1).IsEqualTo(2);
    }
}
