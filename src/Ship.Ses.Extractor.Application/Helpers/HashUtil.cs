using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ship.Ses.Extractor.Application.Helpers
{
    public class HashUtil
    {
        public string ComputeRowHash(IDictionary<string, object> row)
        {
            using var sha256 = SHA256.Create();
            var raw = string.Join("|", row.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}"));
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash); // .NET 5+
        }

    }
}
