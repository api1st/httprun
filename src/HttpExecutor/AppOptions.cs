using System.Collections.Generic;
using CommandLine;
using HttpExecutor.Abstractions;

namespace HttpExecutor
{
    public class AppOptions : IAppOptions
    {
        [Value(0, Required = true, HelpText = "The filename of the .http script.", MetaName = "Filename")]
        public string Filename { get; set; }

        [Option('s', "success-codes", Required = false, Default = new []{ 200, 201, 202, 203, 204, 205, 206, 300, 301, 302, 303, 305 }, HelpText = "Response status codes that allow script to continue with next request.")]
        public IEnumerable<int> SuccessCodes { get; set; }

        [Option('c', "confirm-notes", Required = false, Default = false, HelpText = "Auto confirm all '# @note' confirmations.")]
        public bool AutoConfirmNotes { get; set; }

        [Option('f', "follow-300", Required = false, Default = false, HelpText = "Automatically follow 3xx responses.")]
        public bool Follow300Responses { get; set; }

        [Option('t', "request-timeout", Required = false, Default = 60000, HelpText = "Timeout (ms) for http requests.")]
        public int RequestTimeout { get; set; }

        [Option('d', "delay", Required = false, Default = 200, HelpText = "Delay (ms) between http requests.")]
        public int DelayBetweenRequests { get; set; }

        [Option('u', "user-agent", Required = false, Default = "httprun", HelpText = "User-Agent header sent with requests.")]
        public string UserAgent { get; set; }

        [Option('U', "add-user-agent", Required = false, Default = true, HelpText = "Add a User-Agent header to all requests that do not specify one explicitly.")]
        public bool AddUserAgent { get; set; }

        [Option('v', "terminate-on-variable-error", Required = false, Default = true, HelpText = "Terminate script on variable resolution failure.")]
        public bool TerminateOnVariableResolutionFailure { get; set; }

        [Option('e', "terminate-on-file-error", Required = false, Default = false, HelpText = "Terminate script on suspected file access failure.")]
        public bool TerminateOnFileAccessFailure { get; set; }
    }
}