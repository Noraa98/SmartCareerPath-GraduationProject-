using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Auth;
using SmartCareerPath.Domain.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartCareerPath.Application.ServicesImplementation.Auth
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 128 / 8; // 128 bits
        private const int KeySize = 256 / 8; // 256 bits
        private const int Iterations = 10000;

        /// <summary>
        /// Hashes password using PBKDF2 with random salt
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize
            );

            // Combine salt and hash
            byte[] hashBytes = new byte[SaltSize + KeySize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);

            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }

        public (string Hash, string Salt) HashPasswordWithSalt(string password)
        {
            byte[] salt = new byte[SaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize
            );

            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        /// <summary>
        /// Verifies password against stored hash
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash, string passwordSalt = null)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
                return false;

            try
            {
                // Convert base64 hash back to bytes
                byte[] hashBytes = Convert.FromBase64String(passwordHash);

                // Extract the salt (first 16 bytes)
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Hash the input password with the same salt
                byte[] hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: Iterations,
                    numBytesRequested: KeySize
                );

                // Compare the hashes
                for (int i = 0; i < KeySize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a random salt (for backward compatibility if needed)
        /// </summary>
        public string GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// Validates password strength
        /// Requirements:
        /// - At least 8 characters
        /// - At least one uppercase letter
        /// - At least one lowercase letter
        /// - At least one digit
        /// - At least one special character
        /// </summary>
        public bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum length
            if (password.Length < 8)
                return false;

            // At least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // At least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // At least one digit
            if (!Regex.IsMatch(password, @"[0-9]"))
                return false;

            // At least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]"))
                return false;

            return true;
        }

        /// <summary>
        /// Gets password strength level
        /// </summary>
        public PasswordStrength GetPasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return PasswordStrength.VeryWeak;

            int score = 0;

            // Length
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;

            // Character variety
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]")) score++;

            return score switch
            {
                >= 7 => PasswordStrength.VeryStrong,
                >= 5 => PasswordStrength.Strong,
                >= 3 => PasswordStrength.Medium,
                >= 1 => PasswordStrength.Weak,
                _ => PasswordStrength.VeryWeak
            };
        }

        /// <summary>
        /// Generates a random password
        /// </summary>
        public string GenerateRandomPassword(int length = 16)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            StringBuilder password = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (password.Length < length)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    password.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return password.ToString();
        }
    }

  
}

