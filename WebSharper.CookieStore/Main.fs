namespace WebSharper.CookieStore

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    module Enum = 
        let SameSite = 
            Pattern.EnumStrings "SameSite" [
                "strict" 
                "lax"
                "none"
            ]

    let CookieStoreOptions =
        Pattern.Config "CookieStoreOptions" {
            Required = []
            Optional = [
                "name", T<string>
                "value", T<string>
                "path", T<string>
                "expires", T<float>
                "domain", T<string>
                "sameSite", T<string>
                "secure", T<bool>
                "partitioned", T<bool>
                "maxAge", T<float>
            ]
        }

    // The options dictionary accepted by set(options), matching MDN's CookieStoreSetOptions
    // (the CookieInit dictionary in the WHATWG IDL). name and value are required.
    let CookieStoreSetOptions =
        Pattern.Config "CookieStoreSetOptions" {
            Required = [
                "name", T<string>
                "value", T<string>
            ]
            Optional = [
                "path", T<string>
                "expires", T<float>
                "domain", T<string>
                "sameSite", T<string>
                "secure", T<bool>
                "partitioned", T<bool>
                "maxAge", T<float>
            ]
        }

    // The object returned by get() and getAll(). The spec only guarantees name and value;
    // MDN documents the remaining fields as non-standard but present in browsers.
    let CookieListItem =
        Pattern.Config "CookieListItem" {
            Required = []
            Optional = [
                "name", T<string>
                "value", T<string>
                "path", T<string>
                "expires", T<float>
                "domain", T<string>
                "sameSite", T<string>
                "secure", T<bool>
                "partitioned", T<bool>
            ]
        }

    let CookieStoreDeleteOptions = 
        Pattern.Config "CookieStoreDeleteOptions" {
            Required = []
            Optional = [
                "name", T<string>
                "domain", T<string>
                "path", T<string>
                "partitioned", T<bool>
            ]
        }

    let CookieStoreGetOptions = 
        Pattern.Config "CookieStoreGetOptions" {
            Required = []
            Optional = [
                "name", T<string>
                "url", T<string>
            ]
        }

    let CookieChangeOptions =
        Pattern.Config "CookieChangeOptions" {
            Required = []
            Optional = [
                "changed", !| CookieListItem
                "deleted", !| CookieListItem
            ]
        }

    let CookieChangeEvent =
        Class "CookieChangeEvent"
        |=> Inherits T<Dom.Event> 
        |+> Static [
            Constructor (!?T<string>?``type`` * !?CookieChangeOptions?options)
        ]
        |+> Instance [
            "changed" =? !| CookieListItem
            "deleted" =? !| CookieListItem
        ]

    let ExtendableCookieChangeEvent =
        Class "ExtendableCookieChangeEvent"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor (!?T<string>?``type`` * !?CookieChangeOptions?options)
        ]
        |+> Instance [
            "changed" =? !| CookieListItem
            "deleted" =? !| CookieListItem
        ]
    
    let CookieStore =
        Class "CookieStore"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            // get(name) / get(options): resolves with a single cookie or null.
            "get" => T<string>?name ^-> T<Promise<_>>[CookieListItem]
            "get" => !?CookieStoreGetOptions?options ^-> T<Promise<_>>[CookieListItem]

            // set(name, value) / set(options)
            "set" => T<string>?name * T<string>?value ^-> T<Promise<unit>>
            "set" => CookieStoreSetOptions?options ^-> T<Promise<unit>>

            // delete(name) / delete(options)
            "delete" => T<string>?name ^-> T<Promise<unit>>
            "delete" => CookieStoreDeleteOptions?options ^-> T<Promise<unit>>

            // getAll(name) / getAll(options): resolves with an array of cookies.
            "getAll" => T<string>?name ^-> T<Promise<_>>[!|CookieListItem]
            "getAll" => !?CookieStoreGetOptions?options ^-> T<Promise<_>>[!|CookieListItem]

            "onchange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnChange instead"
            "onchange" =@ CookieChangeEvent ^-> T<unit>
            |> WithSourceName "OnChange"
        ]

    let Subscriptions =
        Pattern.Config "Subscriptions" {
            Required = []
            Optional = [
                "name", T<string> 
                "url", T<string> 
            ]
        }
    
    let CookieStoreManager =
        Class "CookieStoreManager"
        |+> Instance [
            "getSubscriptions" => T<unit> ^-> T<Promise<_>>[Subscriptions] 
            "subscribe" => Subscriptions?subscriptions ^-> T<Promise<unit>> 
            "unsubscribe" => Subscriptions?subscriptions ^-> T<Promise<unit>> 
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.CookieStore" [
                CookieStoreManager
                Subscriptions
                CookieStore
                ExtendableCookieChangeEvent
                CookieChangeEvent
                CookieChangeOptions
                CookieStoreGetOptions
                CookieStoreDeleteOptions
                CookieStoreOptions
                CookieStoreSetOptions
                CookieListItem
                Enum.SameSite
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
