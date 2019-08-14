using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace digitek.brannProsjektering
{
    public class DmnConverter
    {

        /// <summary>
        /// Regex to get the number from string with comparation characters = >,< 
        /// </summary>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        public static string GetComparisonNumber(string cellValue)
        {
            var regex = Regex.Match(cellValue, @"^[<,>][=]?\s?(?<number>\d+[\.]?(\d+)?)$");
            return regex.Success ? regex.Groups["number"].Value : null;
        }

        /// <summary>
        /// Get the number from a range string format
        /// </summary>
        /// <param name="cellValue"></param>
        /// <returns></returns>
        public static string[] GetRangeNumber(string cellValue)
        {
            var regex = Regex.Match(cellValue, @"^[\[,\],]\s?(?<range1>\d+(\.\d+)?).{2}?(?<range2>\d+(\.\d+)?)[\[,\]]$");
            return regex.Success ? new[] { regex.Groups["range1"].Value, regex.Groups["range2"].Value } : null;
        }
    }
}
