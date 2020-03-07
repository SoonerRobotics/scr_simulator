using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StringUtilities
{
    private static Random random = new Random();

    /// <summary>
    /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
    /// </summary>
    /// <param name="length">The length of the string that will be generated</param>
    /// <returns>A randomly generated string</returns>
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}