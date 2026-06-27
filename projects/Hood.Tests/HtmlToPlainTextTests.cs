using Hood.Extensions;
using Xunit;

namespace Hood.Tests
{
    /// <summary>
    /// HOOD-139: emails must ship a real text/plain alternative instead of the C# type name. The text part is
    /// derived from the rendered HTML via <see cref="StringExtensions.HtmlToPlainText"/> when a builder-API
    /// MailObject.Text is not available — these cover that conversion.
    /// </summary>
    public class HtmlToPlainTextTests
    {
        [Fact]
        public void StripsTagsAndKeepsTextContent()
        {
            var html = "<p>Hello <strong>world</strong></p>";
            Assert.Equal("Hello world", html.HtmlToPlainText());
        }

        [Fact]
        public void DecodesHtmlEntities()
        {
            Assert.Equal(
                "Tom & Jerry — café",
                "<p>Tom &amp; Jerry &mdash; caf&eacute;</p>".HtmlToPlainText()
            );
        }

        [Fact]
        public void RendersLinkAsLabelAndUrl()
        {
            var html = "<p>Visit <a href=\"https://hooddigital.com\">our site</a> today</p>";
            Assert.Equal("Visit our site (https://hooddigital.com) today", html.HtmlToPlainText());
        }

        [Fact]
        public void CollapsesLinkWhenLabelEqualsHref()
        {
            var html = "<a href=\"https://hooddigital.com\">https://hooddigital.com</a>";
            Assert.Equal("https://hooddigital.com", html.HtmlToPlainText());
        }

        [Fact]
        public void DropsStyleAndScriptBlocksEntirely()
        {
            var html =
                "<head><style>p{color:red}</style></head><body><script>alert(1)</script><p>Body</p></body>";
            Assert.Equal("Body", html.HtmlToPlainText());
        }

        [Fact]
        public void BlockTagsAndBrBecomeNewlines()
        {
            var html = "<h1>Title</h1><p>Line one<br>Line two</p>";
            Assert.Equal("Title\nLine one\nLine two", html.HtmlToPlainText());
        }

        [Fact]
        public void OutputNeverContainsAngleBracketTags()
        {
            var html = "<div><table><tr><td><p>Cell <em>x</em></p></td></tr></table></div>";
            var text = html.HtmlToPlainText();
            Assert.DoesNotContain("<", text);
            Assert.DoesNotContain(">", text);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void PassesThroughEmptyOrNull(string input)
        {
            Assert.Equal(input, input.HtmlToPlainText());
        }
    }
}
