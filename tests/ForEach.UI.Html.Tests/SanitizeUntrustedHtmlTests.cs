namespace ForEach.UI.Html.Tests;

public sealed class SanitizeUntrustedHtmlTests
{
    private readonly SafeHtmlRenderer _renderer = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmptyInputGivesEmptyString(string? html)
    {
        _renderer.SanitizeUntrustedHtml(html).Should().BeEmpty();
    }

    [Fact]
    public void SafeMarkupSurvives()
    {
        var result = _renderer.SanitizeUntrustedHtml("<p>Hello <strong>world</strong></p>");

        result.Should().Be("<p>Hello <strong>world</strong></p>");
    }

    [Fact]
    public void ScriptElementIsRemoved()
    {
        var result = _renderer.SanitizeUntrustedHtml("<p>a</p><script>alert(1)</script>");

        result.Should().NotContainEquivalentOf("script");
        result.Should().NotContain("alert");
        result.Should().Contain("<p>a</p>");
    }

    [Theory]
    [InlineData("<img src=\"x\" onerror=\"alert(1)\">")]
    [InlineData("<a href=\"https://example.com\" onclick=\"alert(1)\">x</a>")]
    [InlineData("<div onmouseover=\"alert(1)\">x</div>")]
    public void EventHandlerAttributesAreRemoved(string html)
    {
        var result = _renderer.SanitizeUntrustedHtml(html);

        result.Should().NotContainEquivalentOf("onerror");
        result.Should().NotContainEquivalentOf("onclick");
        result.Should().NotContainEquivalentOf("onmouseover");
    }

    [Theory]
    [InlineData("<a href=\"javascript:alert(1)\">x</a>")]
    [InlineData("<a href=\"JaVaScRiPt:alert(1)\">x</a>")]
    [InlineData("<a href=\"&#106;avascript:alert(1)\">x</a>")]
    [InlineData("<a href=\"&#x6A;avascript:alert(1)\">x</a>")]
    [InlineData("<a href=\"java&#9;script:alert(1)\">x</a>")]
    [InlineData("<a href=\"vbscript:msgbox(1)\">x</a>")]
    [InlineData("<a href=\"data:text/html,<script>alert(1)</script>\">x</a>")]
    public void DangerousUrlSchemesAreRemovedFromHref(string html)
    {
        var result = _renderer.SanitizeUntrustedHtml(html);

        result.Should().NotContainEquivalentOf("javascript:");
        result.Should().NotContainEquivalentOf("vbscript:");
        result.Should().NotContainEquivalentOf("data:text");
        result.Should().NotContain("alert(1)");
    }

    [Fact]
    public void HttpsLinksKeepTheirHref()
    {
        var result = _renderer.SanitizeUntrustedHtml("<a href=\"https://example.com/a\">x</a>");

        result.Should().Contain("href=\"https://example.com/a\"");
    }

    [Theory]
    [InlineData("<form action=\"https://evil.example\"><input name=\"a\"></form>", "<form")]
    [InlineData("<iframe src=\"https://evil.example\"></iframe>", "<iframe")]
    [InlineData("<object data=\"https://evil.example/a.swf\"></object>", "<object")]
    [InlineData("<embed src=\"https://evil.example/a.swf\">", "<embed")]
    public void FormIframeObjectAndEmbedAreRemoved(string html, string forbidden)
    {
        var result = _renderer.SanitizeUntrustedHtml("<p>kept</p>" + html);

        result.Should().NotContainEquivalentOf(forbidden);
        result.Should().Contain("<p>kept</p>");
    }

    [Fact]
    public void SameInstanceCanSanitizeRepeatedly()
    {
        var first = _renderer.SanitizeUntrustedHtml("<p>a</p><script>x</script>");
        var second = _renderer.SanitizeUntrustedHtml("<p>a</p><script>x</script>");

        second.Should().Be(first);
    }
}
