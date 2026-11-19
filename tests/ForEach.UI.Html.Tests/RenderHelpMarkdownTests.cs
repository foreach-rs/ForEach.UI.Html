namespace ForEach.UI.Html.Tests;

public sealed class RenderHelpMarkdownTests
{
    private readonly SafeHtmlRenderer _renderer = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t \n")]
    public void NullEmptyOrWhitespaceInputGivesEmptyString(string? markdown)
    {
        _renderer.RenderHelpMarkdown(markdown).Should().BeEmpty();
    }

    [Fact]
    public void BasicMarkdownIsRendered()
    {
        var result = _renderer.RenderHelpMarkdown("Some **bold** and *italic* and `code`.");

        result.Should().Contain("<strong>bold</strong>");
        result.Should().Contain("<em>italic</em>");
        result.Should().Contain("<code>code</code>");
    }

    [Theory]
    [InlineData("# Title", "title")]
    [InlineData("## Getting started", "getting-started")]
    [InlineData("# 6. The workflow list", "6-the-workflow-list")]
    [InlineData("# 6. The workflow & list", "6-the-workflow--list")]
    [InlineData("### Mixed CASE Heading", "mixed-case-heading")]
    public void HeadingsGetGitHubStyleIds(string markdown, string expectedId)
    {
        var result = _renderer.RenderHelpMarkdown(markdown);

        result.Should().Contain($"id=\"{expectedId}\"");
    }

    [Fact]
    public void TableOfContentsAnchorMatchesTheHeadingId()
    {
        var result = _renderer.RenderHelpMarkdown("[Jump](#6-the-workflow--list)\n\n# 6. The workflow & list");

        result.Should().Contain("href=\"#6-the-workflow--list\"");
        result.Should().Contain("id=\"6-the-workflow--list\"");
    }

    [Fact]
    public void HeadingWithoutLettersOrDigitsGetsNoId()
    {
        var result = _renderer.RenderHelpMarkdown("# !!!");

        result.Should().Contain("<h1>");
        result.Should().NotContain("id=");
    }

    [Fact]
    public void RawScriptHtmlIsEscapedNotExecuted()
    {
        var result = _renderer.RenderHelpMarkdown("<script>alert(1)</script>");

        result.Should().NotContain("<script");
        result.Should().NotContain("</script");
    }

    [Fact]
    public void RawHtmlElementsAreEscapedOrDropped()
    {
        var result = _renderer.RenderHelpMarkdown("text <img src=x onerror=alert(1)> text <iframe src=\"https://evil.example\"></iframe>");

        result.Should().NotContain("<img");
        result.Should().NotContain("<iframe");
        result.Should().Contain("&lt;img");
    }

    [Theory]
    [InlineData("[x](javascript:alert(1))")]
    [InlineData("[x](JAVASCRIPT:alert(1))")]
    [InlineData("[x](&#106;avascript:alert(1))")]
    [InlineData("[x](&#x6A;avascript:alert(1))")]
    [InlineData("[x](data:text/html;base64,PHNjcmlwdD5hbGVydCgxKTwvc2NyaXB0Pg==)")]
    [InlineData("[x](vbscript:msgbox(1))")]
    public void DangerousUrlSchemesAreRemovedFromLinks(string markdown)
    {
        var result = _renderer.RenderHelpMarkdown(markdown);

        result.Should().NotContainEquivalentOf("javascript:");
        result.Should().NotContainEquivalentOf("vbscript:");
        result.Should().NotContainEquivalentOf("data:");
        result.Should().NotContain("alert(1)");
    }

    [Fact]
    public void HttpsLinksKeepTheirHref()
    {
        var result = _renderer.RenderHelpMarkdown("[docs](https://example.com/a)");

        result.Should().Contain("<a href=\"https://example.com/a\">docs</a>");
    }

    [Fact]
    public void BareUrlsAreAutoLinked()
    {
        var result = _renderer.RenderHelpMarkdown("See https://example.com/page for more.");

        result.Should().Contain("<a href=\"https://example.com/page\">https://example.com/page</a>");
    }

    [Fact]
    public void BareDangerousUrlsAreNotAutoLinked()
    {
        var result = _renderer.RenderHelpMarkdown("javascript:alert(1)");

        result.Should().NotContain("href");
    }

    [Fact]
    public void PipeTablesAreRendered()
    {
        var result = _renderer.RenderHelpMarkdown("| a | b |\n|---|---|\n| 1 | 2 |");

        result.Should().Contain("<table>");
        result.Should().Contain("<th>a</th>");
        result.Should().Contain("<td>2</td>");
    }

    [Fact]
    public void SoftLineBreaksBecomeLineBreaks()
    {
        var result = _renderer.RenderHelpMarkdown("one\ntwo");

        result.Should().Contain("<br");
    }

    [Fact]
    public void AttributesOutsideHrefAndIdAreRemoved()
    {
        // Markdig can attach attributes to headings only through its extensions, which this pipeline does not enable,
        // so the sanitizer is exercised through an image (not on the tag allow-list) and link titles.
        var result = _renderer.RenderHelpMarkdown("![alt](https://example.com/a.png)\n\n[x](https://example.com \"title\")");

        result.Should().NotContain("<img");
        result.Should().NotContain("title=");
    }

    [Fact]
    public void ListsAndBlockquotesAreKept()
    {
        var result = _renderer.RenderHelpMarkdown("- a\n- b\n\n1. c\n\n> quoted");

        result.Should().Contain("<ul>");
        result.Should().Contain("<ol>");
        result.Should().Contain("<blockquote>");
    }

    [Fact]
    public void SameInstanceRendersRepeatedlyWithTheSameResult()
    {
        var first = _renderer.RenderHelpMarkdown("# A\n\ntext");
        var second = _renderer.RenderHelpMarkdown("# A\n\ntext");

        second.Should().Be(first);
    }
}
