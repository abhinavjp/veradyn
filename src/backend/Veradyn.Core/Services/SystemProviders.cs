using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json; // Using Newtonsoft as the concrete implementation
using Veradyn.Core.Interfaces.Providers;

namespace Veradyn.Core.Services
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftJsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }

    public class SystemSecureRandom : ISecureRandom
    {
        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue) return minValue;

            long diff = (long)maxValue - minValue;
            while (true)
            {
                byte[] fourBytes = new byte[4];
                _rng.GetBytes(fourBytes);
                uint rand = BitConverter.ToUInt32(fourBytes, 0);

                long max = (1L + uint.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }

        public byte[] GetBytes(int count)
        {
            byte[] bytes = new byte[count];
            _rng.GetBytes(bytes);
            return bytes;
        }
    }

    public class SystemCryptoProvider : ICryptoProvider
    {
        private readonly ISecureRandom _random;

        public SystemCryptoProvider(ISecureRandom random)
        {
            _random = random;
        }

        public string ComputeHash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public string ComputeHmacSha256(string input, string key)
        {
            var keyBytes = Convert.FromBase64String(key); // Assuming key is base64
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = hmac.ComputeHash(bytes);
                return Convert.ToBase64String(hash); // URL Safe? standard B64 for now
            }
        }

        public string GenerateSecureToken(int length = 32)
        {
            var bytes = _random.GetBytes(length);
            return ReplacePlusSlash(Convert.ToBase64String(bytes));
        }

        // Simple PBKDF2 wrapper or similar could go here. 
        // For this task, we'll use a fast SHA256 hash for passwords (demo purpose) 
        // OR better: PBKDF2. Let's use PBKDF2 for "Standards Aligned".
        public string HashPassword(string password)
        {
            // Simple approach for this task: Salt + Hash
            // In prod, use PBKDF2 with iteration count.
            // keeping it simple but structured.
            var salt = _random.GetBytes(16);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                var hash = pbkdf2.GetBytes(20);
                var hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string hash, string password)
        {
            try
            {
                var hashBytes = Convert.FromBase64String(hash);
                var salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
                {
                    var key = pbkdf2.GetBytes(20);
                    for (int i = 0; i < 20; i++)
                    {
                        if (hashBytes[i + 16] != key[i]) return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string ReplacePlusSlash(string b64)
        {
            // Simple Url Safe conversion
            return b64.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
