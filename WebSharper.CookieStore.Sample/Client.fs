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
    let cookieStore = As<Window>(JS.Window).CookieStore

    let setCookie() = promise {
        do! cookieStore.Set("username", "JohnDoe", CookieStoreOptions(
            Expires = float (Date.Now() + 86400 * 1000)
        ))
        statusMessage.Value <- "Cookie Set: username=JohnDoe"
    }

    let getCookie() = promise {
        let! cookie = cookieStore.Get("username")
        if isNull(cookie) then
            statusMessage.Value <- "No cookie found!"
        else 
            statusMessage.Value <- $"Cookie Found: {cookie.Name}={cookie.Value}"
    }

    let deleteCookie() = promise {
        do! cookieStore.Delete("username")
        statusMessage.Value <- "Cookie Deleted!"
    }

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
