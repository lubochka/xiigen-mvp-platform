using Xiigen.Auth;

namespace Xiigen.Auth.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public async Task HashAsync_StoresNoPlaintext_And_VerifyMatches()
    {
        var hasher = new Pbkdf2PasswordHasherService();

        var hash = await hasher.HashAsync("correct horse battery staple");
        var verification = await hasher.VerifyAsync("correct horse battery staple", hash.Data!.Hash);

        Assert.True(hash.IsSuccess);
        Assert.DoesNotContain("correct horse", hash.Data.Hash, StringComparison.Ordinal);
        Assert.True(verification.IsSuccess);
        Assert.True(verification.Data!.Matches);
    }
}
