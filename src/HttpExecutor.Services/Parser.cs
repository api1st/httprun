using System;
using System.Linq;
using HttpExecutor.Abstractions;

namespace HttpExecutor.Services
{
    public class Parser : IParser
    {
        private readonly ILineParser _lineParser;

        public Parser(ILineParser lineParser)
        {
            _lineParser = lineParser ?? throw new ArgumentNullException(nameof(lineParser));
        }

        public HttpFile Parse(string[] lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            var output = new HttpFile();

            var currentBlock = new ExecutionBlock();

            IBlockLine parsedLine = null;

            var lineNumber = 0;

            foreach (var line in lines)
            {
                lineNumber++;

                var parsedLineTry = _lineParser.Parse(line, parsedLine, lineNumber);

                if (parsedLineTry == null || parsedLineTry.LineType == LineType.Comment)
                {
                    continue;
                }

                parsedLine = parsedLineTry;

                switch (parsedLine.LineType)
                {
                    case LineType.Divider:
                        output.Blocks.Add(currentBlock);
                        currentBlock = new ExecutionBlock();
                        break;

                    default:
                        currentBlock.Lines.Add(parsedLine);
                        break;
                }
            }

            if (currentBlock.Lines.Any())
            {
                output.Blocks.Add(currentBlock);
            }
            
            return output;
        }
    }
}
