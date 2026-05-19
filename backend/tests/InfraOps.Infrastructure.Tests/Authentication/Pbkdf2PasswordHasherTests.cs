using InfraOps.Infrastructure.Authentication;

namespace InfraOps.Infrastructure.Tests.Authentication;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Should_VerifyPassword_When_HashWasGeneratedFromSamePassword()
    {
        var passwordHasher = new Pbkdf2PasswordHasher();

        var passwordHash = passwordHasher.Hash("InfraOps.Admin!123");

        var result = passwordHasher.Verify("InfraOps.Admin!123", passwordHash);

        Assert.True(result);
    }
}
