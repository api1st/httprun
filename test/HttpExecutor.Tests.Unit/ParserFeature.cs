using System;
using HttpExecutor.Abstractions;
using HttpExecutor.Services;
using Moq;
using Xunit;

namespace HttpExecutor.Tests.Unit
{
    public class ParserFeature
    {
        private Parser _subject;

        private Mock<ILineParser> _mockLineParser;
        private Mock<IBlockLine> _mockParseResult;

        public ParserFeature()
        {
            _mockParseResult = new Mock<IBlockLine>();
            _mockParseResult.Setup(x => x.LineType).Returns(LineType.Divider);

            _mockLineParser = new Mock<ILineParser>();

            _mockLineParser.Setup(x => x.Parse(It.IsAny<string>(), It.IsAny<IBlockLine>(), It.IsAny<int>()))
                .Returns(() => _mockParseResult.Object);

            _subject = new Parser(_mockLineParser.Object);
        }

        [Fact]
        public void Parse_NoLines_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _subject.Parse(null));
        }

        [Fact]
        public void Parse_parses_each_line()
        {
            _subject.Parse(new[] {"one", "two", "three"});

            _mockLineParser.Verify(x => x.Parse("one", It.IsAny<IBlockLine>(), It.IsAny<int>()), Times.Once);
            _mockLineParser.Verify(x => x.Parse("two", It.IsAny<IBlockLine>(), It.IsAny<int>()), Times.Once);
            _mockLineParser.Verify(x => x.Parse("three", It.IsAny<IBlockLine>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Parse_adds_new_block_for_each_divider()
        {
            var result = _subject.Parse(new[] { "one", "two", "three" });

            Assert.Equal(3, result.Blocks.Count);
        }
    }
}
