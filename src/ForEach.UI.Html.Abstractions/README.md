# ForEach.UI.Html.Abstractions

`IHtmlRenderer`: the contract for turning untrusted HTML and markdown into safe HTML (for example for a
Blazor `MarkupString`). No package dependencies, so a UI project can reference only this and load the
implementation, [`ForEach.UI.Html`](https://www.nuget.org/packages/ForEach.UI.Html), on demand.

```csharp
public interface IHtmlRenderer
{
    string SanitizeUntrustedHtml(string? html);
    string RenderHelpMarkdown(string? markdown);
    string RenderChatMarkdown(string? markdown);
}
```

Targets net8.0 and net10.0 (browser-compatible, usable from Blazor WebAssembly on net11.0).
Source and release notes: https://github.com/foreach-rs/ForEach.UI.Html
