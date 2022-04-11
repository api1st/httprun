using System;
using System.Collections.Generic;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class LineParser : ILineParser
    {
        private readonly ILineTypeDecoder _lineTypeDecoder;
        private readonly IRequestVerbLineDecoder _requestVerbLineDecoder;

        public LineParser(ILineTypeDecoder lineTypeDecoder, IRequestVerbLineDecoder requestVerbLineDecoder)
        {
            _lineTypeDecoder = lineTypeDecoder ?? throw new ArgumentNullException(nameof(lineTypeDecoder));
            _requestVerbLineDecoder = requestVerbLineDecoder ?? throw new ArgumentNullException(nameof(requestVerbLineDecoder));
        }

        public IBlockLine Parse(string line, IBlockLine lastLine, int lineNumber)
        {
            var nameParser = new RequestNameParser();
            var requestVerbParser = new RequestVerbParser();
            var requestVerbMultilineParser = new RequestVerbMultilineParser();
            var variableParser = new VariableParser();
            var requestHeaderParser = new RequestHeaderParser();
            var requestDividerParser = new DividerParser();
            var commentParser = new CommentParser();
            var requestBodyParser = new RequestBodyParser();
            var confirmationParser = new NoteParser();

            var tests = new List<IRegexParser>();

            var lastType = _lineTypeDecoder.LastSignificantLineType(lastLine);

            // Ignore blank lines unless in the message body
            if (lastType != LineType.RequestBody)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return null;
                }
            }

            switch (lastType)
            {
                case LineType.Divider:
                    tests.Add(confirmationParser);
                    tests.Add(nameParser);
                    tests.Add(variableParser);
                    tests.Add(requestVerbParser);
                    tests.Add(requestVerbMultilineParser);
                    tests.Add(commentParser);
                    tests.Add(requestDividerParser);
                    break;

                case LineType.RequestVerb:
                    tests.Add(requestHeaderParser);
                    if (_requestVerbLineDecoder.RequestVerbHasBody(lastLine))
                    {
                        tests.Add(requestBodyParser);
                    }
                    else
                    {
                        tests.Add(variableParser);
                        tests.Add(commentParser);
                        tests.Add(requestDividerParser);
                    }
                    break;

                case LineType.RequestVerbMultiLine:
                    tests.Add(requestVerbMultilineParser);
                    tests.Add(requestHeaderParser);
                    if (_requestVerbLineDecoder.RequestVerbHasBody(lastLine))
                    {
                        tests.Add(requestBodyParser);
                    }
                    else
                    {
                        tests.Add(variableParser);
                        tests.Add(commentParser);
                        tests.Add(requestDividerParser);
                    }
                    break;

                case LineType.RequestHeader:
                    tests.Add(requestHeaderParser);
                    if (_requestVerbLineDecoder.RequestVerbHasBody(lastLine))
                    {
                        // It is possible to not specify the body (0 length, no data)
                        tests.Add(requestDividerParser);
                        tests.Add(requestBodyParser);
                    }
                    else
                    {
                        tests.Add(variableParser);
                        tests.Add(commentParser);
                        tests.Add(requestDividerParser);
                    }
                    break;

                case LineType.RequestBody:
                    tests.Add(requestDividerParser);
                    tests.Add(requestBodyParser);
                    break;
            }

            return Test(line, lastLine, tests, lineNumber);
        }

        private IBlockLine Test(string value, IBlockLine previous, ICollection<IRegexParser> tests, int lineNumber)
        {
            foreach (var test in tests)
            {
                if (test.IsMatch(value))
                {
                    return test.Decode(value, previous, lineNumber);
                }
            }

            throw new Exception($"Unexpected line item. '{value}' at line {lineNumber}.");
        }
    }
}