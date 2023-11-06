using System.Collections.Generic;

namespace HttpExecutor.Abstractions
{
    public interface IAppOptions
    {
        string Filename { get; set; }

        IEnumerable<int> SuccessCodes { get; }

        bool AutoConfirmNotes { get; }

        bool Follow300Responses { get; }

        int RequestTimeout { get; }

        int DelayBetweenRequests { get; }

        string UserAgent { get; }

        bool AddUserAgent { get; }

        bool TerminateOnVariableResolutionFailure { get; }

        bool TerminateOnFileAccessFailure { get; }

        bool SkipSslValidation { get; }
    }
}