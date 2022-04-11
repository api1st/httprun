using System;
using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class TimeStampVariableResolver : IDynamicVariableResolver
    {
        private readonly IDateTimeNowProvider _dateTimeNowProvider;
        private const string VariableRegex = "{{\\$timestamp(?: (-?\\d*) ([smhdywM]|ms))?}}";

        public TimeStampVariableResolver(IDateTimeNowProvider dateTimeNowProvider)
        {
            _dateTimeNowProvider = dateTimeNowProvider ?? throw new ArgumentNullException(nameof(dateTimeNowProvider));
        }

        public bool Resolvable(string content)
        {
            var regex = new Regex(VariableRegex);
            return regex.IsMatch(content);
        }

        public string Resolve(string content)
        {
            var regex = new Regex(VariableRegex);
            var matches = regex.Match(content);

            var now = _dateTimeNowProvider.UtcNow();

            if (matches.Groups.Count == 3 && string.IsNullOrWhiteSpace(matches.Groups[1].Value))
            {
                return regex.Replace(content, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(), 1);
            }

            var offset = int.Parse(matches.Groups[1].Value);
            var option = matches.Groups[2].Value;

            DateTime date;
            
            switch (option)
            {
                case "ms":
                    date = now.AddMilliseconds(offset);
                    break;
                case "s":
                    date = now.AddSeconds(offset);
                    break;
                case "m":
                    date = now.AddMinutes(offset);
                    break;
                case "h":
                    date = now.AddHours(offset);
                    break;

                default:
                case "d":
                    date = now.AddDays(offset);
                    break;

                case "w":
                    date = now.AddDays(7 * offset);
                    break;

                case "M":
                    date = now.AddMonths(offset);
                    break;

                case "y":
                    date = now.AddYears(offset);
                    break;
            }

            var unixTime = ((DateTimeOffset)date).ToUnixTimeSeconds();

            return regex.Replace(content, unixTime.ToString(), 1);
        }
    }
}