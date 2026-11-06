# WebSharper Cookie Store API Binding

[![NuGet](https://img.shields.io/nuget/v/WebSharper.CookieStore.svg)](https://www.nuget.org/packages/WebSharper.CookieStore)

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Cookie Store API](https://developer.mozilla.org/en-US/docs/Web/API/Cookie_Store_API), the asynchronous, promise-based replacement for `document.cookie`.

The Cookie Store API is [Baseline 2025](https://developer.mozilla.org/en-US/docs/Web/API/Cookie_Store_API) (newly available since June 2025). It is available in windows and in service workers, and it is only available in **secure contexts** (HTTPS, or `localhost` during development).

## Live demos

- [Sample on GitHub Pages](https://dotnet-websharper.github.io/CookieStore/)
- [Source repository](https://github.com/dotnet-websharper/CookieStore)
- [WebSharper bindings](https://websharper.com/bindings)

## Projects

| Project | Description |
| --- | --- |
| `WebSharper.CookieStore` | The WIG binding (`netstandard2.0`). |
| `WebSharper.CookieStore.Extensions` | Adds the `JS.Window.CookieStore` extension (`netstandard2.0`). |
| `WebSharper.CookieStore.Sample` | ASP.NET Core SPA sample (`net10.0`). |

## Install

```sh
dotnet add package WebSharper.CookieStore
```

## Usage

The binding exposes `CookieStore`, `CookieListItem`, `CookieStoreGetOptions`,
`CookieStoreSetOptions`, `CookieStoreDeleteOptions`, `CookieChangeEvent`,
`ExtendableCookieChangeEvent`, `CookieStoreManager`, `Subscriptions` and the
`SameSite` enum. `Window.cookieStore` is added by the Extensions project:

```fsharp
open WebSharper.CookieStore

let cookieStore = JS.Window.CookieStore
```

The sample (`WebSharper.CookieStore.Sample/Client.fs`) is a complete WebSharper.UI
version using `Var`s and template holes. Its core operations are:

```fsharp
// Set a cookie. Use the options object to pass expires/maxAge:
// set(name, value, options) ignores its third argument in the browser API.
do! cookieStore.Set(
        CookieStoreSetOptions(
            "username",
            "JohnDoe",
            Expires = float (Date.Now() + 86400 * 1000)
        )
    )

// Read one cookie (null when it does not exist):
let! cookie = cookieStore.Get("username")

// List all visible cookies:
let! all = cookieStore.GetAll()

// Delete a cookie:
do! cookieStore.Delete("username")
```

The sample also binds the two inputs and the status line to `Var`s and renders
the cookie list through a template hole:

```fsharp
IndexTemplate.Main()
    .Status(statusMessage.V)
    .nameInput(nameInput)
    .valueInput(valueInput)
    .setCookie(fun _ -> async { do! setCookie().AsAsync() } |> Async.Start)
    // ...
    .Cookies(
        cookies.View.DocSeqCached(fun (cookie: CookieListItem) ->
            Doc.Element "li" [] [ Doc.TextNode $"{cookie.Name} = {cookie.Value}" ]
        )
    )
    .Doc()
|> Doc.RunById "main"
```

## API notes

- **Asynchronous and promise-based.** All methods return `Promise`s; use the
  `promise { ... }` builder.
- **Secure context only.** Calling the API over plain HTTP on a non-local origin
  throws a `SecurityError`.
- **Baseline 2025.** The core `CookieStore` API works in current browsers since
  June 2025, but older browsers may not support it. `CookieStoreManager`
  (service-worker change subscriptions) has more limited availability. Check the
  [MDN compatibility table](https://developer.mozilla.org/en-US/docs/Web/API/Cookie_Store_API#browser_compatibility).
- **Two forms per method.**
  - `get(name)` or `get(options)` resolves to a single `CookieListItem` or `null`.
  - `getAll(name)` or `getAll(options)` resolves to an array of `CookieListItem`.
  - `set(name, value)` or `set(options)`.
  - `delete(name)` or `delete(options)`.
- **Options object for attributes.** `CookieStoreSetOptions` includes `name`,
  `value`, `path`, `expires`, `domain`, `sameSite`, `secure`, `partitioned` and
  `maxAge`. `expires` is a Unix timestamp in milliseconds; `maxAge` is in
  seconds. Setting both causes a `TypeError`.
- **Returned cookies.** `get` and `getAll` return `CookieListItem`s. The spec
  guarantees `name` and `value`; MDN documents `domain`, `expires`,
  `partitioned`, `path`, `sameSite` and `secure` as non-standard fields that
  browsers may expose.
- **Same-origin and scope.** Cookies are subject to the same-origin policy. The
  `url` option of `CookieStoreGetOptions` only matches the document's URL in a
  window (or the worker's origin in a service worker).
- **Change events.** `CookieStore` inherits `EventTarget`; use
  `AddEventListener("change", ...)` or the `OnChange` property. The event is a
  `CookieChangeEvent` whose `Changed` and `Deleted` lists contain
  `CookieListItem`s.

## Run the sample

```sh
dotnet build WebSharper.CookieStore/WebSharper.CookieStore.fsproj
dotnet run --project WebSharper.CookieStore.Sample
```

Open the printed URL (HTTPS, or `localhost`) and use the inputs and buttons to
set, get, list and delete cookies. The GitHub Pages demo is built from the same
project.

## Build from source

```sh
git clone https://github.com/dotnet-websharper/CookieStore.git
cd CookieStore
dotnet build WebSharper.CookieStore.sln -c Release
```

## Deployment

The sample is deployed to GitHub Pages by
`.github/workflows/ghpages.yml`. The workflow builds
`WebSharper.CookieStore.Sample` in Release on `ubuntu-latest` with the .NET 10
SDK and publishes `WebSharper.CookieStore.Sample/wwwroot` to the `gh-pages`
branch.

## Repository layout

```
WebSharper.CookieStore/             # the WIG binding
  Main.fs                           # types + [<assembly: Extension>] registration
WebSharper.CookieStore.Extensions/  # Window.cookieStore extension
  Library.fs
WebSharper.CookieStore.Sample/      # ASP.NET Core SPA sample
  Client.fs
  wwwroot/index.html                # template (ws-onclick / ws-var / ws-hole)
  wsconfig.json
.github/workflows/ghpages.yml       # build + deploy the sample to gh-pages
```

## License

Apache 2.0 — see [LICENSE.md](LICENSE.md).
