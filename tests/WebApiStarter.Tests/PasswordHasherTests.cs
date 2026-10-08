using Infrastructure.Security;
using Xunit;

namespace WebApiStarter.Tests;

// Unit tests for the PBKDF2 password hasher: hashing must use a random salt
// (so identical passwords hash differently) and verification must only
// accept the exact original password.
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("S3cret-P@ss");

        Assert.True(_hasher.Verify("S3cret-P@ss", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("S3cret-P@ss");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Each hash embeds a fresh random salt, so two hashes of the same
        // password must never be identical. Attackers can't spot reused passwords.
        var first = _hasher.Hash("same-password");
        var second = _hasher.Hash("same-password");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_WithMalformedStoredHash_ReturnsFalse()
    {
        Assert.False(_hasher.Verify("anything", "not-a-valid-hash"));
    }

    [Fact]
    public void Verify_IsCaseSensitive()
    {
        var hash = _hasher.Hash("Password123");

        Assert.False(_hasher.Verify("password123", hash));
    }
}
