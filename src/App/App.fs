module App

open Feliz
open Feliz.JSX
open Fable.Core
open Components

// Entry point must be in a separate file
// for Vite Hot Reload to work

[<RequireQualifiedAccess>]
type Tab =
    | Counter
    | Svg
    | Sketch
    // | Shoelace
    member this.Name =
        match this with
        | Counter -> "Counter"
        | Svg -> "Svg"
        | Sketch -> "Sketch"
        // | Shoelace -> "Shoelace Web Components"

[<JSX.Component>]
let TabEl(tab: Tab, activeTab, setActiveTab) =
    Html.li [
        Attr.classes ["is-active", tab = activeTab]
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
    let activeTab, setActiveTab = React.useState(Tab.Sketch)
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
                        // TabEl(Tab.Shoelace, activeTab, setActiveTab)
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
                // | Tab.Shoelace -> Shoelace.App()
            )]
        ]
    ]


open Browser
open Browser.Types
open Fable.React
open Fable.Core.JsInterop

type ReactRoot =
    abstract render: ReactElement -> unit

let createRoot (el: HTMLElement): ReactRoot = importMember "react-dom/client"
let inline toReact (jsxEl: JSX.Element): ReactElement = unbox jsxEl

let root = createRoot(document.getElementById("app-container"))
root.render(Tabs() |> toReact)
