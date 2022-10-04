module Components

open Browser.Types
open Feliz
open Feliz.JSX.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.React

let asTuple (stateHook: IStateHook<'T>) =
    stateHook.current, fun (v: 'T) -> stateHook.update(v)

[<JSX.Component>]
let Div (classes: string seq) children =
    Html.div [
        Attr.classList classes
        Html.children children
    ]

[<JSX.Component>]
let Counter() =
    let count, setCount = Hooks.useState(0) |> asTuple

    let doubled() = count * 2
    let quadrupled() = doubled() * 2

    Html.fragment [
        Html.p $"{count} * 2 = {doubled()}"
        Html.p $"{doubled()} * 2 = {quadrupled()}"
        Html.br []
        Html.button [
            Attr.className "button"
            Ev.onClick(fun _ -> count + 1 |> setCount)
            Html.children [
                Html.text $"Click me!"
            ]
        ]
    ]

[<JSX.Component>]
let Svg() =
    let gradient, setGradient = Hooks.useState(40) |> asTuple

    Html.fragment [
        Html.div [
            Html.input [
                Attr.typeRange
                Attr.min 1
                Attr.max 100
                Attr.value $"{gradient}"
                Ev.onTextInput (fun (value: string) -> value |> int |> setGradient)
            ]
        ]

        Html.p "This is created using Feliz API"

        Svg.svg [
            Attr.height 150
            Attr.width 300
            Svg.children [
                Svg.defs [
                    Svg.linearGradient [
                        Attr.id "gr1"
                        Attr.x1 (length.perc 0)
                        Attr.y1 (length.perc 60)
                        Attr.x2 (length.perc 100)
                        Attr.y2 (length.perc 0)
                        Svg.children [
                            Svg.stop [
                                Attr.offset (length.perc gradient)
                                Attr.stopColor "rgb(255,255,3)"
                                Attr.stopOpacity 1
                            ]
                            Svg.stop [
                                Attr.offset (length.perc 100)
                                Attr.stopColor "rgb(255,0,0)"
                                Attr.stopOpacity 1
                            ]
                        ]
                    ]
                ]
                Svg.ellipse [
                    Attr.cx 125
                    Attr.cy 75
                    Attr.rx 100
                    Attr.ry 60
                    Attr.fill "url(#gr1)"
                ]
            ]
        ]

        Html.p "This is created using a JSX template"
        // Note the interpolation hole must replace the whole attribute value (as in standard JSX)
        // We cannot interpolate only part of the attribute, e.g. `offset="{gradient}%%"

        JSX.html $"""
        <svg height="150" width="300">
            <defs>
                <linearGradient id="gr2" x1="0%%" y1="60%%" x2="100%%" y2="0%%">
                <stop offset={length.perc gradient} stopColor="rgb(52, 235, 82)" stopOpacity="1" />
                <stop offset="100%%" stopColor="rgb(52, 150, 235)" stopOpacity="1" />
                </linearGradient>
            </defs>
            <ellipse cx="125" cy="75" rx="100" ry="60" fill="url(#gr2)" />
            Sorry but this browser does not support inline SVG.
        </svg>
        """
    ]

module Sketch =
    let setStyle (el: HTMLElement) ((key, value): string * string) =
        el?style?(key) <- value

    let maxGridPixelWidth = 500.

    let randomHexColorString(): string =
        let v = JS.Math.random() * 16777215. |> int
        "#" + System.Convert.ToString(v, 16)

    let clampGridSideLength(newSideLength) =
        min (max newSideLength 0.) 100.

    [<JSX.Component>]
    let App(initialSide: float) =
        let gridSideLength, setGridSideLength = Hooks.useState(initialSide) |> asTuple
        let gridTemplateString = Hooks.useMemo((fun () ->
            $"repeat({gridSideLength}, {maxGridPixelWidth / gridSideLength}px)"), [|gridSideLength|])

        Html.fragment [
            Html.div [
                Attr.style [ Css.marginBottom 10 ]
                Html.children [
                    Html.label "Grid Side Length: "
                    Html.input [
                        Attr.typeNumber
                        Attr.value (gridSideLength.ToString())
                        Ev.onInput(fun e ->
                            (e.currentTarget :?> HTMLInputElement).valueAsNumber
                            |> clampGridSideLength
                            |> setGridSideLength
                        )
                    ]
                ]
            ]

            Html.div [
                Attr.style [
                    Css.displayGrid
                    Css.gridTemplateRows [gridTemplateString |> grid.ofString]
                    Css.gridTemplateColumns [gridTemplateString |> grid.ofString]
                ]
                Html.children (
                    Array.init (gridSideLength ** 2 |> int) id
                    |> Array.map (fun i ->
                        Html.div [
                            Attr.key i
                            Attr.className "cell"
                            Ev.onMouseEnter(fun ev ->
                                let el = ev.currentTarget :?> HTMLElement
                                Css.backgroundColor(randomHexColorString()) |> setStyle el
                                JS.setTimeout (fun () ->
                                    Css.backgroundColor "initial" |> setStyle el) 500
                                |> ignore
                            )
                        ]
                    )
                )
            ]
        ]

module Shoelace =

    [<JSX.Component>]
    let ImageComparer() =
        let SlImageComparer = importMember "@shoelace-style/shoelace/dist/react"
        let position, setPosition = Hooks.useState(25) |> asTuple

        Html.fragment [
            Html.div [
                Html.input [
                    Attr.typeRange
                    Attr.min 1
                    Attr.max 100
                    Attr.value (position |> string)
                    Ev.onTextInput (fun (value: string) ->
                        value |> int |> setPosition)
                ]
            ]

            // We can instantiate imported JSX components "dynamically" without bindings
            JSX.create SlImageComparer [
                "position" ==> position
                "onSlChange" ==> (fun (ev: Event) ->
                    let pos: int = ev.target?position
                    setPosition pos
                )
                Html.children [
                    Html.img [
                        Attr.slot "before"
                        Attr.src "https://images.unsplash.com/photo-1520903074185-8eca362b3dce?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1200&q=80"
                        Attr.alt "A person sitting on bricks wearing untied boots."
                    ]
                    Html.img [
                        Attr.slot "after"
                        Attr.src "https://images.unsplash.com/photo-1520640023173-50a135e35804?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=2250&q=80"
                        Attr.alt "A person sitting on a yellow curb tying shoelaces on a boot."
                    ]
                ]
            ]
        ]

    [<JSX.Component>]
    let QrCode() =
        let value, setValue = Hooks.useState("https://shoelace.style/") |> asTuple

        Html.fragment [
            Html.input [
                Attr.className "input mb-5"
                Attr.typeText
                Attr.autoFocus true
                Attr.value (value)
                Ev.onTextChange setValue
            ]
            Html.div [
                JSX.jsx $"""
                import {{ SlQrCode }} from "@shoelace-style/shoelace/dist/react"
                <SlQrCode value={value} radius="0.5"></SlQrCode>
                """
            ]
        ]

    [<JSX.Component>]
    let App() =
        Html.fragment [
            QrCode()
            Html.br []
            ImageComparer()
        ]
