namespace ForEach.UI.Html;

/// <summary>
/// Turns untrusted HTML and markdown into safe HTML, for example for a Blazor <c>MarkupString</c>.
/// The default implementation is <c>SafeHtmlRenderer</c> in the ForEach.UI.Html package (Markdig and
/// HtmlSanitizer). A UI project can reference only this contract and load the implementation on demand.
/// </summary>
public interface IHtmlRenderer
{
    /// <summary>
    /// Sanitizes untrusted HTML (inbound email bodies, user-supplied content) with the library's default
    /// allow-list minus form, iframe, object, embed and javascript: URLs. Empty for null or empty input.
    /// </summary>
    string SanitizeUntrustedHtml(string? html);

    /// <summary>
    /// Renders a help topic: Markdig (raw HTML disabled, auto links, pipe tables, soft breaks as line breaks),
    /// GitHub-style heading ids, then a strict allow-list sanitizer (href and id only, http/https only).
    /// </summary>
    string RenderHelpMarkdown(string? markdown);

    /// <summary>
    /// Renders a chat answer: the same Markdig pipeline, then a strict allow-list sanitizer
    /// (href only, http/https only). The caller rewrites the surviving links.
    /// </summary>
    string RenderChatMarkdown(string? markdown);
}
