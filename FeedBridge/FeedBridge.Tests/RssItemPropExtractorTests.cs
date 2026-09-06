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
        [InlineData("District.9.2009.BDRip.XviD.Asd-Full", "District 9")]
        public void ExtractTitle_ReturnsTitleWithoutReleaseInformation(string input, string expected)
        {
            var result = _propExtractor.ExtractTitle(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("This is the title", null)]
        [InlineData("This.is.the.title.2024.BDRip.x264.Asd-Full", null)]
        [InlineData("This.is.the.title.2024.1080p.BDRip.x264.Asd-Full", "1080p")]
        [InlineData("This.is.the.title.2024.720p.AMZN.WEB-DL.DDP2.0.H.264.Asd-Full", "720p")]
        public void ExtractQuality_ReturnsOnlyQualityInformation(string input, string? expected)
        {
            var result = _propExtractor.ExtractQuality(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("This is the title", null)]
        [InlineData("This.is.the.title.2024.BDRip.x264.Asd-Full", 2024)]
        [InlineData("Title.2024.BDRip.x264.Asd-Full", 2024)]
        [InlineData("District.9.2009.BDRip.XviD.Asd-Full", 2009)]
        public void ExtractReleaseYear_ReturnOnlyReleaseYear(string input, int? expected)
        {
            var result = _propExtractor.ExtractReleaseYear(input);

            Assert.Equal(expected, result);
        }
    }
}