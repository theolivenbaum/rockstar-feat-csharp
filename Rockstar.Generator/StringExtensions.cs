using System;

namespace Rockstar.Generator
{
    internal static class StringExtensions
    {
        public static string ToJavaScriptString(this string instr)
        {
            return Uri.EscapeDataString(instr).Replace("'", @"\'").Replace(@"""", @"\""");
        }
    }
}
