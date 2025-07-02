using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Helpers
{
    public static class PhoneHelper
    {
        public static string NormalizeToE164Format(string rawPhone, string? defaultCountryCode = "234")
        {
            if (string.IsNullOrWhiteSpace(rawPhone))
                return rawPhone;

            var digits = new string(rawPhone.Where(char.IsDigit).ToArray());

            // If starts with 00, replace with +
            if (rawPhone.StartsWith("00"))
                return "+" + digits.Substring(2);

            // If already in + format
            if (rawPhone.StartsWith("+"))
                return rawPhone;

            // If starts with country code (e.g., 234 or 44) and is long enough
            if (digits.Length >= 10 && defaultCountryCode == null)
                return "+" + digits;

            // If local number with leading zero and default country code provided
            if (digits.StartsWith("0") && defaultCountryCode != null)
                return $"+{defaultCountryCode}{digits.Substring(1)}";

            // If defaultCountryCode is provided, treat as full number without leading 0
            if (defaultCountryCode != null && !digits.StartsWith(defaultCountryCode))
                return $"+{defaultCountryCode}{digits}";

            return "+" + digits;
        }

    }

}
