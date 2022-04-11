using System;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class ConfirmationReader : IConfirmationReader
    {
        private readonly IConsole _console;
        private readonly IAppOptions _options;

        public ConfirmationReader(IConsole console, IAppOptions options)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public bool Confirm(IHttpRequest request)
        {
            if (!request.RequiresConfirmation)
            {
                return true;
            }

            if (_options.AutoConfirmNotes)
            {
                return true;
            }

            _console.WriteLine(request.ToString());

            _console.WriteLine("");
            _console.WriteLine("Confirm execution of above request? [y/n]: ");

            if (_console.ReadLine().ToLower() == "y")
            {
                return true;
            }

            return false;
        }
    }
}