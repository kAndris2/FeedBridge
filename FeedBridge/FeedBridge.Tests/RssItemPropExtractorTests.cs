using FeedBridge.Services;

namespace FeedBridge.Tests
{
    public class RssItemPropExtractorTests
    {
        private readonly RssItemPropExtractor _propExtractor = new();

        [Theory]
        [InlineData("This is the title", "This is the title")]
        [InlineData("This is the title BDRip", "This is the title")]
        [InlineData("This.is.the.title.2024.BDRip.x264.Asd-Full", "This is the title")]
        public void ExtractTitle_ReturnsTitleWithoutReleaseInformation(string input, string expected)
        {
            var result = _propExtractor.ExtractTitle(input);

            Assert.Equal(expected, result);
        }
    }
}