using System;

namespace Veradyn.Core.Interfaces.Providers
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public interface IJsonSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string json);
    }

    public interface ICryptoProvider
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
        string ComputeHash(string input); // SHA256 usually
        string GenerateSecureToken(int length = 32);
        string ComputeHmacSha256(string input, string key);
    }

    public interface ISecureRandom
    {
        int Next(int minValue, int maxValue);
        byte[] GetBytes(int count);
    }
}
