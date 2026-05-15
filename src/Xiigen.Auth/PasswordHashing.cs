using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Xiigen.Kernel;

namespace Xiigen.Auth;

public sealed record PasswordHashResult(string Hash, string Algorithm, int WorkFactor);

public sealed record PasswordVerificationResult(bool Matches, bool NeedsRehash);

public interface IPasswordHasherService
{
    Task<DataProcessResult<PasswordHashResult>> HashAsync(
        string plaintext,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<PasswordVerificationResult>> VerifyAsync(
        string plaintext,
        string hash,
        CancellationToken cancellationToken = default);
}

public sealed class Pbkdf2PasswordHasherService : IPasswordHasherService
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 120_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public Task<DataProcessResult<PasswordHashResult>> HashAsync(
        string plaintext,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            return Task.FromResult(DataProcessResult<PasswordHashResult>.Failure(
                "VALIDATION_FAILED",
                "Password is required"));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            plaintext,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashBytes);
        var encoded = string.Join(
            "$",
            Algorithm,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));

        return Task.FromResult(DataProcessResult<PasswordHashResult>.Success(
            new PasswordHashResult(encoded, Algorithm, Iterations)));
    }

    public Task<DataProcessResult<PasswordVerificationResult>> VerifyAsync(
        string plaintext,
        string hash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plaintext) || string.IsNullOrWhiteSpace(hash))
        {
            return Task.FromResult(DataProcessResult<PasswordVerificationResult>.Failure(
                "VALIDATION_FAILED",
                "Password and hash are required"));
        }

        var parts = hash.Split('$');
        if (parts.Length != 4 ||
            !string.Equals(parts[0], Algorithm, StringComparison.Ordinal) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations))
        {
            return Task.FromResult(DataProcessResult<PasswordVerificationResult>.Failure(
                "HASH_FORMAT_INVALID",
                "Password hash format is invalid"));
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            plaintext,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);
        var matches = CryptographicOperations.FixedTimeEquals(expected, actual);

        return Task.FromResult(DataProcessResult<PasswordVerificationResult>.Success(
            new PasswordVerificationResult(matches, iterations < Iterations)));
    }
}

public static class XiigenAuthServiceCollectionExtensions
{
    public static IServiceCollection AddXiigenAuth(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasherService, Pbkdf2PasswordHasherService>();
        return services;
    }
}
