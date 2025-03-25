# WebSharper CookieStore API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [CookieStore API](https://developer.mozilla.org/en-US/docs/Web/API/Cookie_Store_API), enabling seamless management of cookies in WebSharper applications.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the CookieStore API.

2. **Sample Project**:
   - Demonstrates how to use the CookieStore API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/CookieStore/).

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.CookieStore
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/CookieStore.git
   cd CookieStore
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.CookieStore/WebSharper.CookieStore.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.CookieStore.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/CookieStore/](https://dotnet-websharper.github.io/CookieStore/)

## Example Usage

Below is an example of how to use the CookieStore API in a WebSharper project:

```fsharp
namespace WebSharper.CookieStore.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.CookieStore

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let statusMessage = Var.Create "No cookie set."
    let cookieStore = JS.Window.CookieStore

    // Function to set a cookie
    let setCookie() = promise {
        do! cookieStore.Set("username", "JohnDoe", CookieStoreOptions(
            Expires = float (Date.Now() + 86400 * 1000) // Cookie expires in 1 day
        ))
        statusMessage.Value <- "Cookie Set: username=JohnDoe"
    }

    // Function to get a cookie
    let getCookie() = promise {
        let! cookie = cookieStore.Get("username")
        if isNull(cookie) then
            statusMessage.Value <- "No cookie found!"
        else
            statusMessage.Value <- $"Cookie Found: {cookie.Name}={cookie.Value}"
    }

    // Function to delete a cookie
    let deleteCookie() = promise {
        do! cookieStore.Delete("username")
        statusMessage.Value <- "Cookie Deleted!"
    }

    // Function to handle cookie changes
    let cookieChanges() =
        cookieStore.AddEventListener("change", fun (event:Dom.Event) -> printfn($"Cookie Changed: {event}"))

    [<SPAEntryPoint>]
    let Main () =

        cookieChanges()

        IndexTemplate.Main()
            .Status(statusMessage.V)
            .getCookie(fun _ ->
                async {
                    do! getCookie().AsAsync()
                }
                |> Async.Start
            )
            .setCookie(fun _ ->
                async {
                    do! setCookie().AsAsync()
                }
                |> Async.Start
            )
            .deleteCookie(fun _ ->
                async {
                    do! deleteCookie().AsAsync()
                }
                |> Async.Start
            )
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to set, retrieve, and delete cookies using the CookieStore API in a WebSharper project.

## Important Considerations

- **Limited Browser Support**: Some browsers may not fully support the CookieStore API; check [MDN CookieStore API](https://developer.mozilla.org/en-US/docs/Web/API/Cookie_Store_API) for the latest compatibility information.
- **Secure Context Requirement**: The CookieStore API requires HTTPS for secure cookie management.
- **Same-Origin Policy**: Cookies are subject to the same-origin policy, which affects cross-domain cookie access.
