using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Helpers
{
    public static class CreditCardHelpers
    {
        private static Regex paddedCardNumber = new Regex(@"0{12}\d{4}", RegexOptions.Compiled);

        public static string GetCardDigits(string cardBin, string cardLastFourDigits)
        {
            return $"{cardBin?.Trim()}****{cardLastFourDigits?.Trim()}";
        }

        // TODO: card bin can be 8 digits
        public static string GetCardDigits(string cardNumber)
        {
            cardNumber = cardNumber?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            //In some cases we only know 4 last digits, while the rest of the card is just padded 0
            //so we only show relevant part of the card
            if (paddedCardNumber.IsMatch(cardNumber))
            {
                return $"****{cardNumber?.Substring(cardNumber.Length - 4, 4)}";
            }

            if (cardNumber.Length < 6)
            {
                cardNumber = cardNumber.PadRight(6, '0');
            }

            return $"{cardNumber?.Substring(0, 6)}****{cardNumber?.Substring(cardNumber.Length - 4, 4)}";
        }

        // TODO: card bin can be 8 digits
        public static string GetCardBin(string cardNumber)
        {
            cardNumber = cardNumber?.Trim() ?? string.Empty;
            if (cardNumber.Length < 6)
            {
                cardNumber = cardNumber.PadRight(6, '0');
            }

            return cardNumber?.Substring(0, 6);
        }

        public static string GetCardLastFourDigits(string cardNumber)
        {
            cardNumber = cardNumber?.Trim() ?? string.Empty;
            if (cardNumber.Length < 6)
            {
                cardNumber = cardNumber.PadRight(6, '0');
            }

            return $"{cardNumber?.Substring(cardNumber.Length - 4, 4)}";
        }

        public static string GetCardLastFourDigitsWithPrefix(string cardNumber)
        {
            cardNumber = cardNumber?.Trim() ?? string.Empty;
            if (cardNumber.Length < 6)
            {
                cardNumber = cardNumber.PadRight(6, '0');
            }

            return $"****{cardNumber?.Substring(cardNumber.Length - 4, 4)}";
        }

        public static string FixdLastFourDigits(string last4digits)
        {
            last4digits = last4digits?.Trim() ?? string.Empty;
            if (last4digits.Length < 4)
            {
                last4digits = last4digits.PadRight(4, '0');
            }

            return last4digits?.Substring(last4digits.Length - 4, 4);
        }

        public static CardExpiration ParseCardExpiration(string expirationStr)
        {
            if (string.IsNullOrWhiteSpace(expirationStr))
            {
                return null;
            }

            expirationStr = expirationStr.Replace(" ", string.Empty);

            var parts = expirationStr.Split("/");

            if (parts.Length != 2)
            {
                return null;
            }

            int.TryParse(parts[0], out var month);

            if (month < 1 || month > 12)
            {
                return null;
            }

            int.TryParse(parts[1], out var year);

            if (year < 18 || year > 99)
            {
                return null;
            }

            return new CardExpiration() { Year = year, Month = month };
        }

        public static string ParseCardExpiration(CardExpiration cardExp)
        {
            if (cardExp == null)
            {
                return string.Empty;
            }

            DateTime expirationDate = new DateTime(cardExp.Year ?? 1999, cardExp.Month ?? 0, 1);
            return expirationDate.ToString("MM'/'yy");
        }

        public static CardExpiration ParseCardExpiration(DateTime? expirationDate)
        {
            if (expirationDate == null)
            {
                return null;
            }

            return new CardExpiration { Year = expirationDate.Value.Year - 2000, Month = expirationDate.Value.Month };
        }

        public static string FormatCardExpiration(DateTime? expirationDate)
        {
            if (expirationDate == null)
            {
                return null;
            }

            return expirationDate.Value.ToString("MM'/'yy");
        }

        public static string GetCardReference(string cardDigits, string cardOwnerNationalId)
        {
            return $"{cardDigits?.Trim()}-{cardOwnerNationalId?.Trim()}";
        }

        public static string GetCardReference(string cardBin, string cardLastFourDigits, string cardOwnerNationalId)
        {
            var cardDigits = GetCardDigits(cardBin, cardLastFourDigits);
            return GetCardReference(cardDigits, cardOwnerNationalId);
        }

        public static string GetCardHash(string cardNumber, long terminalId, long merchantID, string expiration)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes($"{cardNumber}{terminalId}{merchantID}{expiration}"));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
