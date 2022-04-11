using System.Text.RegularExpressions;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class RequestBodyFileParser : IRequestBodyFileParser
    {
        private const string RegexString = "^<(@[a-zA-Z0-9]*)? (.+)$";

        public (bool isFile, bool exists) IsFileInput(string line)
        {
            var isMatch = new Regex(RegexString).IsMatch(line);

            if (!isMatch)
            {
                // Not a match, return
                return (false, false);
            }

            var matches = new Regex(RegexString).Match(line);

            var filename = matches.Groups[2].Value;

            // Determine if the file exists
            if (System.IO.File.Exists(filename))
            {
                return (true, true);
            }

            return (true, false);
        }

        public IRequestBodyFile FileContent(string line)
        {
            var matches = new Regex(RegexString).Match(line);

            var resolveVariables = !string.IsNullOrWhiteSpace(matches.Groups[1].Value);
            var encoding = matches.Groups[1].Value.TrimStart('@');
            var filename = matches.Groups[2].Value;

            return new RequestBodyFile
            {
                Data = System.IO.File.ReadAllBytes(filename),
                Encoding = encoding,
                ResolveVariables = resolveVariables
            };
        }
    }
}