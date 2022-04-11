using System.Globalization;

namespace HttpExecutor.Services
{
    public class DateJsToDotNetFormatConverter : IDatejsToDotNetFormatConverter
    {
        // https://day.js.org/docs/en/display/format

        /*
         Format	Output	Description
           YY	18	Two-digit year
           YYYY	2018	Four-digit year
           M	1-12	The month, beginning at 1
           MM	01-12	The month, 2-digits
           MMM	Jan-Dec	The abbreviated month name
           MMMM	January-December	The full month name
           D	1-31	The day of the month
           DD	01-31	The day of the month, 2-digits
           d	0-6	The day of the week, with Sunday as 0
           dd	Su-Sa	The min name of the day of the week
           ddd	Sun-Sat	The short name of the day of the week
           dddd	Sunday-Saturday	The name of the day of the week
           H	0-23	The hour
           HH	00-23	The hour, 2-digits
           h	1-12	The hour, 12-hour clock
           hh	01-12	The hour, 12-hour clock, 2-digits
           m	0-59	The minute
           mm	00-59	The minute, 2-digits
           s	0-59	The second
           ss	00-59	The second, 2-digits
           SSS	000-999	The millisecond, 3-digits
           Z	+05:00	The offset from UTC, ±HH:mm
           ZZ	+0500	The offset from UTC, ±HHmm
           A	AM PM	
           a	am pm	
        */

        public string Convert(string format)
        {
            var converted = format;

            // Year
            converted = converted.Replace("YYYY", "yyyy", false, CultureInfo.CurrentCulture);
            converted = converted.Replace("YY", "yy", false, CultureInfo.CurrentCulture);

            // Month - All the same

            // Day
            converted = converted.Replace("DD", "dd", false, CultureInfo.CurrentCulture);
            converted = converted.Replace("D", "d", false, CultureInfo.CurrentCulture);
            // "d", Day of the week, not supported by .net

            // Hours - All the same

            // Minutes - All the same

            // Seconds
            converted = converted.Replace("SSS", "FFF", false, CultureInfo.CurrentCulture);

            // Timezome
            converted = converted.Replace("Z", "zzz", false, CultureInfo.CurrentCulture);
            // 'ZZ' not supported by .net

            // AM/PM designator
            converted = converted.Replace("A", "tt", false, CultureInfo.CurrentCulture);
            converted = converted.Replace("a", "tt", false, CultureInfo.CurrentCulture);

            return converted;
        }
    }
}