using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Extension
{
    public static class NormalizationExtensions
    {
        public static string NormalizeForLookup(this string? input)
        {
            return (input ?? string.Empty).Trim().ToLowerInvariant();
        }
    }
}
