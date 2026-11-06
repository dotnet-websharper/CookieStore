namespace WebSharper.CookieStore.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.CookieStore

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so you can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // Cookie name/value bound to the two text inputs via ws-var.
    let nameInput = Var.Create "username"
    let valueInput = Var.Create "JohnDoe"

    // Status line and the list of cookies shown by the UI.
    let statusMessage = Var.Create "No cookie set."
    let cookies = Var.Create ([]: CookieListItem list)

    let cookieStore = JS.Window.CookieStore

    // Set a cookie from the current input values. The options object form is
    // required for expires/maxAge: set(name, value, options) ignores its third
    // argument in the Cookie Store API.
    let setCookie () =
        promise {
            try
                do! cookieStore.Set(
                        CookieStoreSetOptions(
                            nameInput.Value,
                            valueInput.Value,
                            Expires = float (Date.Now() + 86400 * 1000)
                        )
                    )
                statusMessage := $"Cookie Set: {nameInput.Value}={valueInput.Value}"
            with ex ->
                statusMessage := $"Set failed: {ex.Message}"
        }

    // Read a single cookie by name.
    let getCookie () =
        promise {
            try
                let! cookie = cookieStore.Get(nameInput.Value)
                if isNull cookie then
                    statusMessage := "No cookie found!"
                else
                    statusMessage := $"Cookie Found: {cookie.Name}={cookie.Value}"
            with ex ->
                statusMessage := $"Get failed: {ex.Message}"
        }

    // Delete a cookie by name.
    let deleteCookie () =
        promise {
            try
                do! cookieStore.Delete(nameInput.Value)
                statusMessage := "Cookie Deleted!"
            with ex ->
                statusMessage := $"Delete failed: {ex.Message}"
        }

    // List every cookie visible to this document and refresh the UI list.
    let getAllCookies () =
        promise {
            try
                let! all = cookieStore.GetAll()
                cookies.Value <- List.ofArray all
                statusMessage := $"Found {all.Length} cookie(s)."
            with ex ->
                statusMessage := $"GetAll failed: {ex.Message}"
        }

    // Keep the status line in sync when the cookie store changes.
    let cookieChanges () =
        cookieStore.AddEventListener("change", fun (event: Dom.Event) ->
            statusMessage := "Cookie store changed."
            Console.Log("Cookie changed:", event)
        )

    [<SPAEntryPoint>]
    let Main () =

        cookieChanges()

        IndexTemplate.Main()
            .Status(statusMessage.V)
            .nameInput(nameInput)
            .valueInput(valueInput)
            .setCookie(fun _ ->
                async {
                    do! setCookie().AsAsync()
                }
                |> Async.Start
            )
            .getCookie(fun _ ->
                async {
                    do! getCookie().AsAsync()
                }
                |> Async.Start
            )
            .deleteCookie(fun _ ->
                async {
                    do! deleteCookie().AsAsync()
                }
                |> Async.Start
            )
            .getAllCookies(fun _ ->
                async {
                    do! getAllCookies().AsAsync()
                }
                |> Async.Start
            )
            .Cookies(
                cookies.View.DocSeqCached(fun (cookie: CookieListItem) ->
                    Doc.Element "li" [] [ Doc.TextNode $"{cookie.Name} = {cookie.Value}" ]
                )
            )
            .Doc()
        |> Doc.RunById "main"
