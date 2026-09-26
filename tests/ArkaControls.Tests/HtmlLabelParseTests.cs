using System.Linq;
using Xunit;

namespace ArkaControls.Tests
{
    public class HtmlLabelParseTests
    {
        private static string Flat(string html) =>
            string.Join("|", ArkaHtmlLabel.Parse(html).Select(l => string.Concat(l.Runs.Select(r => r.Text))));

        [Fact]
        public void Plain_text_is_one_run()
        {
            var lines = ArkaHtmlLabel.Parse("Hello");
            var run = Assert.Single(Assert.Single(lines).Runs);
            Assert.Equal("Hello", run.Text);
            Assert.False(run.Bold);
        }

        [Fact]
        public void Bold_tag_marks_only_the_enclosed_text()
        {
            var runs = ArkaHtmlLabel.Parse("a <b>bold</b> c")[0].Runs;
            Assert.Equal(new[] { "a ", "bold", " c" }, runs.Select(r => r.Text));
            Assert.Equal(new[] { false, true, false }, runs.Select(r => r.Bold));
        }

        [Fact]
        public void Nested_styles_combine()
        {
            var run = ArkaHtmlLabel.Parse("<b><i>x</i></b>")[0].Runs.Single();
            Assert.True(run.Bold);
            Assert.True(run.Italic);
            Assert.False(run.Underline);
        }

        [Theory]
        [InlineData("a<br>b", "a|b")]
        [InlineData("a<br/>b", "a|b")]
        [InlineData("a\nb", "a|b")]
        public void Line_breaks_split_lines(string html, string expected) => Assert.Equal(expected, Flat(html));

        [Fact]
        public void Div_align_right_applies_to_its_content()
        {
            var lines = ArkaHtmlLabel.Parse("<div align='right'>₹ 1,200</div>");
            var line = lines.First(l => l.Runs.Count > 0);
            Assert.Equal(System.Windows.Forms.HorizontalAlignment.Right, line.Align);
            Assert.Equal("₹ 1,200", line.Runs.Single().Text);
        }

        [Fact]
        public void Unclosed_tags_do_not_throw()
        {
            Assert.Equal("open", Flat("<b>open"));
            Assert.Equal("x", Flat("</b>x"));
        }

        [Fact]
        public void Unknown_markup_is_shown_as_text_never_interpreted()
        {
            Assert.Equal("<script>alert(1)</script>", Flat("<script>alert(1)</script>"));
            Assert.Equal("<a href='x'>link</a>", Flat("<a href='x'>link</a>"));
        }

        [Fact]
        public void Entities_are_decoded()
        {
            Assert.Equal("a < b & \"c\"", Flat("a &lt; b &amp; &quot;c&quot;"));
        }
    }
}
