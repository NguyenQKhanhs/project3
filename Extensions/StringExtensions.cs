using System;

namespace Project3.Extensions // Đảm bảo rằng đây là namespace đúng
{
    public static class StringExtensions
    {
        public static string Shorten(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength) + "...";
        }
    }
}