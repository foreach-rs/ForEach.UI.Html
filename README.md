# ForEach.UI.Html

Safe HTML and markdown rendering for UIs, as two NuGet packages from one repository. Targets net8.0 and
net10.0 and works in Blazor WebAssembly.

| Package | What it holds |
|---|---|
| `ForEach.UI.Html.Abstractions` | `IHtmlRenderer` only. No package dependencies. |
| `ForEach.UI.Html` | `SafeHtmlRenderer`: Markdig + HtmlSanitizer, GitHub-style heading ids, allow-list sanitizing. Depends on the Abstractions package of the same version. |

Each package ships its own README with the details. Sibling of
[Banking](https://github.com/foreach-rs/Banking), with the same layout and release process.

## Layout

```
src/ForEach.UI.Html.Abstractions/
src/ForEach.UI.Html/
tests/ForEach.UI.Html.Tests/
.github/workflows/ci.yml        build + test + pack check on push to main and on pull requests
.github/workflows/publish.yml   on tag v*: test, pack both with the tag's version, push to nuget.org
```

## Releasing

```
git tag v0.1.0
git push origin v0.1.0
```

The tag is the version of both packages (`v0.2.0` becomes `0.2.0`; `v0.2.0-beta.1` becomes a
prerelease). The `<Version>` in each csproj is only the fallback for a local `dotnet pack`. A push
to `main` builds and tests only.

After the first release, set `<PackageValidationBaselineVersion>` in each csproj to that version so
a breaking change against the published API fails the build.

### One-time setup: Trusted Publishing on nuget.org

The publish workflow holds no API key. It authenticates with a GitHub OIDC token that nuget.org
exchanges for a one-hour key, which works only once a policy on nuget.org trusts this workflow:

1. On nuget.org, open your profile menu, Trusted Publishing, and add a policy owned by the account
   that owns the `ForEach.*` packages (the `ForEachRS` organisation):
   - Repository Owner: `foreach-rs`
   - Repository: `ForEach.UI.Html`
   - Workflow File: `publish.yml` (the file name only)
   - Environment: leave empty
2. In this repository, Settings, Secrets and variables, Actions, **Variables**, set
   `NUGET_USER` to the nuget.org profile name that owns the policy (`ForEachRS`).

A policy created for a private repository is only provisionally active for seven days until the
first successful publish; push a tag within that window.

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
