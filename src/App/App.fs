module App

open Feliz
open Feliz.JSX.React
open Fable.Core
open Fable.React
open Components

// Entry point must be in a separate file
// for Vite Hot Reload to work

[<RequireQualifiedAccess>]
type Tab =
    | Counter
    | Svg
    | Sketch
    | Shoelace
    | TodoMVC
    member this.Name =
        match this with
        | Counter -> "Counter"
        | Svg -> "Svg"
        | Sketch -> "Sketch"
        | Shoelace -> "Shoelace Web Components"
        | TodoMVC -> "Todo MVC"

[<JSX.Component>]
let TabEl(tab: Tab, activeTab, setActiveTab) =
    Html.li [
        Attr.classList ["is-active", tab = activeTab]
        Html.children [
            Html.a [
                Ev.onClick (fun _ -> tab |> setActiveTab)
                Html.children [
                    Html.text tab.Name
                ]
            ]
        ]
    ]

[<JSX.Component>]
let Tabs() =
    let activeTab, setActiveTab = Hooks.useState(Tab.Sketch) |> asTuple
    Html.fragment [
        Html.div [
            Attr.className "tabs"
            Attr.style [
                Css.marginBottom (length.rem 0.5)
            ]
            Html.children [
                Html.ul [
                    Html.children [
                        TabEl(Tab.Counter, activeTab, setActiveTab)
                        TabEl(Tab.Svg, activeTab, setActiveTab)
                        TabEl(Tab.Sketch, activeTab, setActiveTab)
                        TabEl(Tab.Shoelace, activeTab, setActiveTab)
                        TabEl(Tab.TodoMVC, activeTab, setActiveTab)
                    ]
                ]
            ]
        ]
        Html.div [
            Attr.style [
                Css.margin (length.zero, length.auto)
                Css.maxWidth 800
                Css.padding 20
            ]
            Html.children [(
                match activeTab with
                | Tab.Counter ->  Counter()
                | Tab.Svg -> Svg()
                | Tab.Sketch -> Sketch.App(10.)
                | Tab.Shoelace -> Shoelace.App()
                | Tab.TodoMVC -> TodoMVC.TodoMVC.App() |> toJsx
            )]
        ]
    ]


open Browser

let root = ReactDomClient.createRoot(document.getElementById("app-container"))
root.render(Tabs() |> toReact)
