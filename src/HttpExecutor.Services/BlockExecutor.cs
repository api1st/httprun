using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class BlockExecutor : IBlockExecutor
    {
        private readonly IVariableProvider _variableProvider;
        private readonly IRequestBuilder _requestBuilder;
        private readonly IRequestSender _requestSender;
        private readonly IConfirmationReader _confirmationReader;
        private readonly IRequestVariableResolver _requestVariableResolver;
        private readonly IAppOptions _appOptions;

        public BlockExecutor(IVariableProvider variableProvider,
            IRequestBuilder requestBuilder,
            IRequestSender requestSender,
            IConfirmationReader confirmationReader,
            IRequestVariableResolver requestVariableResolver,
            IAppOptions appOptions)
        {
            _variableProvider = variableProvider ?? throw new ArgumentNullException(nameof(variableProvider));
            _requestBuilder = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
            _requestSender = requestSender ?? throw new ArgumentNullException(nameof(requestSender));
            _confirmationReader = confirmationReader ?? throw new ArgumentNullException(nameof(confirmationReader));
            _requestVariableResolver = requestVariableResolver ?? throw new ArgumentNullException(nameof(requestVariableResolver));
            _appOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
        }

        public async Task<(IEnumerable<Warning>, IHttpRequest?, IHttpResponse?)> ExecuteAsync(ExecutionBlock block)
        {
            var hasRequest = false;

            _variableProvider.ResetLookupWarnings();

            var warnings = new List<Warning>();

            // Reduce any RequestVerbMultiLines down to a single RequestVerbLine
            var lines = SquashMultiLineVerbs(block.Lines);

            foreach (var line in lines)
            {
                switch (line.LineType)
                {
                    case LineType.UserConfirmation:
                        _requestBuilder.RequireConfirmation();
                        break;

                    case LineType.RequestName:
                        _requestBuilder.SetName(((RequestNameLine)line).Name);
                        break;

                    case LineType.VariableDefinition:
                        var variableLine = (VariableLine)line;
                        var registrationWarning = _variableProvider.Register(variableLine.VariableName, variableLine.VariableValue);
                        if (!string.IsNullOrWhiteSpace(registrationWarning)) warnings.Add(new Warning(registrationWarning, WarningType.DuplicateVariableDeclaration));
                        break;

                    case LineType.RequestVerb:
                    case LineType.RequestHeader:
                    case LineType.RequestBody:
                        hasRequest = true;
                        _requestBuilder.AddLine(line);
                        break;
                }
            }

            if (hasRequest)
            {
                var request = _requestVariableResolver.ResolveVariables(_requestBuilder.Build());
                
                if (_confirmationReader.Confirm(request))
                {
                    warnings.AddRange(_variableProvider.UnresolvedVariableLookups.Select(x => new Warning(x, WarningType.VariableResolutionFailure)));

                    if (_appOptions.TerminateOnVariableResolutionFailure &&
                        _variableProvider.UnresolvedVariableLookups.Any())
                    {
                        // Don't execute the request.
                        return (warnings, request, null);
                    }

                    var (req, res) = await _requestSender.SendAsync(request);
                    return (warnings, req, res);
                }
            }
            else
            {
                warnings.AddRange(_variableProvider.UnresolvedVariableLookups.Select(x => new Warning(x, WarningType.VariableResolutionFailure)));
            }

            return (warnings, null, null);
        }

        private static ICollection<IBlockLine> SquashMultiLineVerbs(ICollection<IBlockLine> lines)
        {
            var multiLines = lines.Where(x => x.LineType == LineType.RequestVerbMultiLine);

            var output = new List<IBlockLine>();

            var concatinatedLine = "";
            
            foreach (var line in multiLines)
            {
                concatinatedLine += line.Raw;
            }
            
            IBlockLine? previous = null;

            var createdVerbLine = false;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines.ElementAt(i);

                if (line.LineType == LineType.RequestVerbMultiLine)
                {
                    if (!createdVerbLine)
                    {
                        createdVerbLine = true;
                        line = new RequestVerbLine(concatinatedLine, previous, lines.First().LineNumber);
                        output.Add(line);
                        previous = line;
                    }
                }
                else
                {
                    if (createdVerbLine)
                    {
                        // The next item after the new verb line needs to have it's previous property adjusted.
                        line.Previous = previous;
                    }

                    previous = line;
                    output.Add(line);
                }
            }

            return output;
        }
    }
}
