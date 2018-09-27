using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjectiveC.Model
{
    public static class StringExtension
    {
        public static string StartWithUppercase(this string original)
        {

            return string.IsNullOrWhiteSpace(original) || string.IsNullOrEmpty(original)
                ? original
                : char.ToUpper(original[0]) + original.Substring(1);;
        }
    }
}
