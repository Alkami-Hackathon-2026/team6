using System;

namespace Alkami.Monitoring.NewRelic
{
    /// <summary>
    /// Helpful extensions for strings
    /// </summary> 
    public static class StringExtensions
    {
        /// <summary>
        /// Verifies if the string contains the substring using the StringComparison protocol provided
        /// This extension is copied over from the .NET Core implementation
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static bool Contains(this string str, string substring,
            StringComparison comp)
        {
            if (substring == null)
            {
                throw new ArgumentNullException("substring", "substring cannot be null.");
            }
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
            {
                throw new ArgumentException("comp is not a member of StringComparison", "comp");
            }

            return str.IndexOf(substring, comp) >= 0;
        }
    }
}
