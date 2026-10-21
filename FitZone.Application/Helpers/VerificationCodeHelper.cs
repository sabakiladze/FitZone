using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Application.Helpers
{
    public static class VerificationCodeHelper
    {
        public static string Generate() =>
           RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        public static string Hash(string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    }
}
