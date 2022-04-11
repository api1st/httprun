using System.Text.RegularExpressions;

namespace HttpExecutor.Services
{
    public class RequestContentTypeMultipartMimeParser : IRequestContentTypeMultipartMimeParser
    {
        private const string RegexString = "^multipart/form-data; boundary=(.{1,69})$";

        public string GetBoundary(string line)
        {
            var matches = new Regex(RegexString, RegexOptions.IgnoreCase).Match(line);

            if (!matches.Success)
            {
                return string.Empty;
            }

            return matches.Groups[1].Value;
        }
    }
}