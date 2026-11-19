# ForEach.UI.Html

`SafeHtmlRenderer`: Markdig and an allow-list HtmlSanitizer behind `IHtmlRenderer`
(from `ForEach.UI.Html.Abstractions`).

```csharp
IHtmlRenderer renderer = new SafeHtmlRenderer();

string help = renderer.RenderHelpMarkdown("# 6. The workflow & list\n\nSee [docs](https://example.com).");
string chat = renderer.RenderChatMarkdown(answerFromAssistant);
string mail = renderer.SanitizeUntrustedHtml(inboundEmailBody);
```

| Member | What it does |
|---|---|
| `SanitizeUntrustedHtml` | HtmlSanitizer defaults minus `form`, `iframe`, `object`, `embed` and `javascript:` URLs. Empty for null or empty input. |
| `RenderHelpMarkdown` | Markdig (raw HTML disabled, auto links, pipe tables, soft breaks as line breaks), GitHub-style heading ids (`6. The workflow & list` becomes `6-the-workflow--list`), then an allow-list sanitizer: `href` and `id` only, http/https only. |
| `RenderChatMarkdown` | The same pipeline without heading ids: `href` only, http/https only. |

`SafeHtmlRenderer` is `public sealed` with a public parameterless constructor, so a host that loads the
assembly lazily can create it through reflection. Create one instance and share it.

Targets net8.0 and net10.0 (browser-compatible, usable from Blazor WebAssembly on net11.0).
Source and release notes: https://github.com/foreach-rs/ForEach.UI.Html
