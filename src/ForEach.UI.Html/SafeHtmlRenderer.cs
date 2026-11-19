using System.Text;
using Ganss.Xss;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ForEach.UI.Html;

/// <summary>
/// Markdig + allow-list HtmlSanitizer behind <see cref="IHtmlRenderer"/>. Create it once and share it:
/// the sanitizers are built per instance and are thread-safe for sanitizing. It keeps a public parameterless
/// constructor so a host that loads this assembly lazily can create it through reflection.
/// </summary>
public sealed class SafeHtmlRenderer : IHtmlRenderer
{
    // Raw HTML disabled, bare URLs auto-linked, pipe tables, soft breaks kept as line breaks. One pipeline
    // for help topics and assistant answers.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseAutoLinks()
        .UsePipeTables()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    private static readonly string[] MarkdownTags =
    [
        "p", "br", "strong", "em", "b", "i", "ul", "ol", "li",
        "h1", "h2", "h3", "h4", "h5", "h6", "hr",
        "code", "pre", "blockquote", "a",
        "table", "thead", "tbody", "tr", "th", "td",
    ];

    private readonly HtmlSanitizer _untrustedHtmlSanitizer = CreateUntrustedHtmlSanitizer();

    // Heading ids (assigned in AssignHeadingIds) are the scroll targets for the help topic's in-page TOC.
    private readonly HtmlSanitizer _helpSanitizer = CreateMarkdownSanitizer("href", "id");

    // Assistant content echoes DB strings, so nothing outside the allow-list (and no javascript:/data: URL)
    // may survive.
    private readonly HtmlSanitizer _chatSanitizer = CreateMarkdownSanitizer("href");

    /// <inheritdoc />
    public string SanitizeUntrustedHtml(string? html)
        => string.IsNullOrEmpty(html) ? string.Empty : _untrustedHtmlSanitizer.Sanitize(html);

    /// <inheritdoc />
    public string RenderHelpMarkdown(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var document = Markdown.Parse(markdown, Pipeline);
        AssignHeadingIds(document);

        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();

        return _helpSanitizer.Sanitize(writer.ToString());
    }

    /// <inheritdoc />
    public string RenderChatMarkdown(string? markdown)
        => string.IsNullOrWhiteSpace(markdown)
            ? string.Empty
            : _chatSanitizer.Sanitize(Markdown.ToHtml(markdown, Pipeline));

    // Give every heading a GitHub-style id, e.g. "6. The workflow list" -> "6-the-workflow-list".
    // The help markdown's tables of contents are authored with GitHub-style anchors that keep the
    // leading number (and the double hyphen from "&"), whereas Markdig's own UseAutoIdentifiers drops
    // the number ("the-workflow-list") and collapses hyphens, so the TOC links never matched their
    // targets. Generating the ids ourselves the same way GitHub does keeps them in sync.
    private static void AssignHeadingIds(MarkdownDocument document)
    {
        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            var slug = GitHubSlug(GetInlineText(heading.Inline));
            if (slug.Length > 0)
            {
                heading.GetAttributes().Id = slug;
            }
        }
    }

    private static string GetInlineText(ContainerInline? container)
    {
        if (container is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var literal in container.Descendants<LiteralInline>())
        {
            builder.Append(literal.Content.ToString());
        }

        return builder.ToString();
    }

    // Mirrors GitHub's heading slug: lower-case, drop everything that is not a letter, digit, space or
    // hyphen, then turn each space into a hyphen (consecutive hyphens are preserved, as GitHub does).
    private static string GitHubSlug(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (ch is ' ' or '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString();
    }

    private static HtmlSanitizer CreateMarkdownSanitizer(params string[] attributes)
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in MarkdownTags)
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in attributes)
        {
            sanitizer.AllowedAttributes.Add(attribute);
        }

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");

        return sanitizer;
    }

    private static HtmlSanitizer CreateUntrustedHtmlSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        // Library defaults already strip <script>, event handlers, javascript: URIs, etc.
        // Disallow form/iframe/object/embed for inbound email rendering.
        sanitizer.AllowedTags.Remove("form");
        sanitizer.AllowedTags.Remove("iframe");
        sanitizer.AllowedTags.Remove("object");
        sanitizer.AllowedTags.Remove("embed");
        sanitizer.AllowedSchemes.Remove("javascript");
        return sanitizer;
    }
}