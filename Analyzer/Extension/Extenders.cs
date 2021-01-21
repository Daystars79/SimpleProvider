using System;
using System.Linq;

namespace SimpleProvider.Analyzer.Extension
{
    /// <summary>
    ///     Extensions
    /// </summary>
    public static class Extenders
    {
        /// <summary>
        ///     Convert string to an Pascal Case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            // Replace all non-letter and non-digits with an underscore and lowercase the rest.
            string sample = string.Join("",
                str.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

            // Split the resulting string by underscore
            // Select first character, uppercase it and concatenate with the rest of the string
            var arr = sample
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}");

            // Join the resulting collection
            sample = string.Join("", arr);

            return sample;
        }
    }
}