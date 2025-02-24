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
            Required = [
                "name", T<string>
                "url", T<string>
            ]
            Optional = []
        }

    let CookieChangeOptions =
        Pattern.Config "CookieChangeOptions" {
            Required = []
            Optional = [
                "changed", !| CookieStoreOptions // Array of changed cookies
                "deleted", !| CookieStoreOptions // Array of deleted cookies
            ]
        }

    let CookieChangeEvent =
        Class "CookieChangeEvent"
        |=> Inherits T<Dom.Event> 
        |+> Static [
            Constructor (!?T<string>?``type`` * !?CookieChangeOptions?options)
        ]
        |+> Instance [
            "changed" =? !| CookieStoreOptions  
            "deleted" =? !| CookieStoreOptions 
        ]

    let ExtendableCookieChangeEvent =
        Class "ExtendableCookieChangeEvent"
        |=> Inherits T<Dom.Event>
        |+> Static [
            Constructor (!?T<string>?``type`` * !?CookieChangeOptions?options)
        ]
        |+> Instance [
            "changed" =? !| CookieStoreOptions // Returns an array containing the changed cookies
            "deleted" =? !| CookieStoreOptions // Returns an array containing the deleted cookies
        ]
    
    let CookieStore =
        Class "CookieStore"
        |=> Inherits T<Dom.EventTarget>
        |+> Instance [
            "get" => !?T<string>?name * !?CookieStoreGetOptions?options ^-> T<Promise<_>>[CookieStoreOptions]
            "set" => !?T<string>?name * !?T<string>?value * !?CookieStoreOptions?options ^-> T<Promise<unit>>
            "delete" => !?T<string>?name * !?CookieStoreDeleteOptions?options ^-> T<Promise<unit>>
            "getAll" => !?T<string>?name * !?CookieStoreGetOptions?options ^-> T<Promise<_>>[!|CookieStoreOptions]

            "onchange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnChange instead"
            "onchange" =@ CookieChangeEvent ^-> T<unit>
            |> WithSourceName "OnChange"
        ]

    let Subscriptions =
        Pattern.Config "Subscriptions" {
            Required = []
            Optional = [
                "name", T<string> // Array of changed cookies
                "url", T<string> // Array of deleted cookies
            ]
        }
    
    let CookieStoreManager =
        Class "CookieStoreManager"
        |+> Instance [
            "getSubscriptions" => T<unit> ^-> T<Promise<_>>[Subscriptions] // Returns a Promise resolving to a list of subscriptions
            "subscribe" => Subscriptions?subscriptions ^-> T<Promise<unit>> // Subscribes the service worker
            "unsubscribe" => Subscriptions?subscriptions ^-> T<Promise<unit>> // Unsubscribes the service worker
        ]

    let Window = 
        Class "Window"
        |+> Instance [
            "cookieStore" =? CookieStore
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.CookieStore" [
                Window
                CookieStoreManager
                Subscriptions
                CookieStore
                ExtendableCookieChangeEvent
                CookieChangeEvent
                CookieChangeOptions
                CookieStoreGetOptions
                CookieStoreDeleteOptions
                CookieStoreOptions
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
