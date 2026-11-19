namespace ForEach.UI.Html.Tests;

public sealed class RenderChatMarkdownTests
{
    private readonly SafeHtmlRenderer _renderer = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n \t")]
    public void NullEmptyOrWhitespaceInputGivesEmptyString(string? markdown)
    {
        _renderer.RenderChatMarkdown(markdown).Should().BeEmpty();
    }

    [Fact]
    public void BasicMarkdownIsRendered()
    {
        var result = _renderer.RenderChatMarkdown("**bold** and *italic*\n\n- one\n- two");

        result.Should().Contain("<strong>bold</strong>");
        result.Should().Contain("<em>italic</em>");
        result.Should().Contain("<li>one</li>");
    }

    [Fact]
    public void HeadingsHaveNoIds()
    {
        var result = _renderer.RenderChatMarkdown("# Title");

        result.Should().Contain("<h1>");
        result.Should().NotContain("id=");
    }

    [Fact]
    public void RawScriptHtmlIsNotExecutable()
    {
        var result = _renderer.RenderChatMarkdown("hello <script>alert(1)</script>");

        result.Should().NotContain("<script");
    }

    [Fact]
    public void RawEventHandlerHtmlIsNotExecutable()
    {
        var result = _renderer.RenderChatMarkdown("<img src=x onerror=alert(1)> <a href=\"https://example.com\" onclick=\"alert(1)\">x</a>");

        result.Should().NotContain("<img");
        result.Should().NotContain("<a href=\"https://example.com\" onclick");
    }

    [Theory]
    [InlineData("[x](javascript:alert(1))")]
    [InlineData("[x](JaVaScRiPt:alert(1))")]
    [InlineData("[x](&#106;avascript:alert(1))")]
    [InlineData("[x](&#x6A;avascript:alert(1))")]
    [InlineData("[x](data:text/html,hello)")]
    [InlineData("[x](vbscript:msgbox(1))")]
    public void DangerousUrlSchemesAreRemovedFromLinks(string markdown)
    {
        var result = _renderer.RenderChatMarkdown(markdown);

        result.Should().NotContainEquivalentOf("javascript:");
        result.Should().NotContainEquivalentOf("vbscript:");
        result.Should().NotContainEquivalentOf("data:");
    }

    [Theory]
    [InlineData("[docs](https://example.com/a)", "href=\"https://example.com/a\"")]
    [InlineData("[docs](http://example.com/a)", "href=\"http://example.com/a\"")]
    public void HttpAndHttpsLinksKeepTheirHref(string markdown, string expected)
    {
        _renderer.RenderChatMarkdown(markdown).Should().Contain(expected);
    }

    [Fact]
    public void MailtoLinksLoseTheirHrefAndRelativeLinksSurvive()
    {
        var result = _renderer.RenderChatMarkdown("[a](/relative) [b](mailto:a@example.com)");

        result.Should().Contain("href=\"/relative\"");
        result.Should().NotContain("mailto:");
    }

    [Fact]
    public void BareUrlsAreAutoLinked()
    {
        var result = _renderer.RenderChatMarkdown("Visit https://example.com/page now");

        result.Should().Contain("<a href=\"https://example.com/page\">https://example.com/page</a>");
    }

    [Fact]
    public void PipeTablesAreRendered()
    {
        var result = _renderer.RenderChatMarkdown("| a | b |\n|---|---|\n| 1 | 2 |");

        result.Should().Contain("<table>");
        result.Should().Contain("<td>1</td>");
    }

    [Fact]
    public void SoftLineBreaksBecomeLineBreaks()
    {
        _renderer.RenderChatMarkdown("one\ntwo").Should().Contain("<br");
    }

    [Fact]
    public void ImagesAreNotAllowed()
    {
        _renderer.RenderChatMarkdown("![x](https://example.com/a.png)").Should().NotContain("<img");
    }

    [Fact]
    public void SameInstanceRendersRepeatedlyWithTheSameResult()
    {
        var first = _renderer.RenderChatMarkdown("**a**");
        var second = _renderer.RenderChatMarkdown("**a**");

        second.Should().Be(first);
    }
}
