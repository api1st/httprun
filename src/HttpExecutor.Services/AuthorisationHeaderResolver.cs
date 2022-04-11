using System.Text.RegularExpressions;

namespace HttpExecutor.Services
{
    public class AuthorisationHeaderResolver : IAuthorisationHeaderResolver
    {
        private const string BasicColonRegexString = "^Basic ([^:]*):([^:]*)$";
        private const string BasicSpaceRegexString = "^Basic ([^\\s]*) ([^\\s]*)$";
        private const string DigestRegexString = "^Digest ([^\\s]*) ([^\\s]*)$";

        public AuthorisationHeaderResolver()
        {
            
        }

        public string ProcessAuth(string headerValue, string verb, string uri)
        {
            var colonMatch = new Regex(BasicColonRegexString).Match(headerValue);

            if (colonMatch.Success)
            {
                return "Basic " + GenerateBasicAuth(colonMatch.Groups[1].Value, colonMatch.Groups[2].Value);
            }

            var spaceMatch = new Regex(BasicSpaceRegexString).Match(headerValue);

            if (spaceMatch.Success)
            {
                return "Basic " + GenerateBasicAuth(spaceMatch.Groups[1].Value, spaceMatch.Groups[2].Value);
            }
            
            // Digest process - https://netsecuritydevelopment.wordpress.com/2016/02/16/create-http-digest-authentication-header/
            var digestMatch = new Regex(DigestRegexString).Match(headerValue);

            if (digestMatch.Success)
            {
                // TODO - this may sit better in SendAsync part of RequestSender
            }

            // No substitution
            return headerValue;
        }

        private string GenerateBasicAuth(string user, string password)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(user + ":" + password));
        }
    }
}