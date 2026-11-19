# ForEach.UI.Html

Safe HTML and markdown rendering for UIs, as two NuGet packages from one repository. Targets net8.0 and
net10.0 and works in Blazor WebAssembly.

| Package | What it holds |
|---|---|
| `ForEach.UI.Html.Abstractions` | `IHtmlRenderer` only. No package dependencies. |
| `ForEach.UI.Html` | `SafeHtmlRenderer`: Markdig + HtmlSanitizer, GitHub-style heading ids, allow-list sanitizing. Depends on the Abstractions package of the same version. |

Each package ships its own README with the details. Sibling of
[Banking](https://github.com/foreach-rs/Banking), with the same layout.

## Layout

```
src/ForEach.UI.Html.Abstractions/
src/ForEach.UI.Html/
tests/ForEach.UI.Html.Tests/
.github/workflows/ci.yml        build + test + pack check on push to main and on pull requests
(no publish workflow here: releases go through foreach-rs/nuget-releases)
```

## Releasing

Releases are not published from this repository. They go through the release hub,
[foreach-rs/nuget-releases](https://github.com/foreach-rs/nuget-releases), which publishes every
`ForEach.*` package to nuget.org through one Trusted Publishing policy. This repository only runs
`ci.yml`.

1. Tag the commit and push the tag:

   ```
   git tag v0.1.0
   git push origin v0.1.0
   ```

2. In the hub, open a PR that adds or changes `releases/ForEach.UI.Html.yml`:

   ```yaml
   repository: foreach-rs/ForEach.UI.Html
   tag: v0.1.0
   solution: ForEach.UI.Html.slnx
   ```

   The PR dry-runs restore, build, test and pack. Merging it publishes both packages.

The tag is the version of both packages (`v0.2.0` becomes `0.2.0`; `v0.2.0-beta.1` becomes a
prerelease). The `<Version>` in each csproj is only the fallback for a local `dotnet pack`. A push
to `main` builds and tests only.

After the first release, set `<PackageValidationBaselineVersion>` in each csproj to that version so
a breaking change against the published API fails the build.

## Consuming the packages

Published to nuget.org, so no feed configuration is needed:

```
dotnet add package ForEach.UI.Html
```

A UI project that only needs the contract references `ForEach.UI.Html.Abstractions`.

## Local build

```
dotnet build
dotnet test
dotnet pack -c Release -o packages
```

## License

MIT, see [LICENSE](LICENSE).
