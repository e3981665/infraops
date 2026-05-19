using InfraOps.Infrastructure.Authentication;

namespace InfraOps.Infrastructure.Tests.Authentication;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Should_VerifyPassword_When_HashWasGeneratedFromSamePassword()
    {
        var passwordHasher = new Pbkdf2PasswordHasher();

        var passwordHash = passwordHasher.Hash("DemoOnly-Admin-Local");

        var result = passwordHasher.Verify("DemoOnly-Admin-Local", passwordHash);

        Assert.True(result);
    }
}
