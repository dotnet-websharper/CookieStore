namespace WebSharper.CookieStore

open WebSharper
open WebSharper.JavaScript

[<JavaScript;AutoOpen>]
module Extensions = 
    type Window with
        [<Inline "$this.cookieStore">]
        member this.CookieStore with get():CookieStore = null