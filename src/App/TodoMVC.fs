module TodoMVC

open Feliz
open Feliz.UseElmish
open Browser.Types

module private Elmish =
    open System
    open Elmish

    type Todo = {
        Id: Guid
        Description: string
        Editing: string option
        Completed: bool
    }

    type State =
        { Todos: Todo list }

    type Msg =
        | AddNewTodo of string
        | DeleteTodo of Guid
        | ToggleCompleted of Guid
        | CancelEdit
        | ApplyEdit of string
        | StartEditingTodo of Guid

    let newTodo txt =
        {
            Id = Guid.NewGuid()
            Description = txt
            Completed = false
            Editing = None
        }

    let initTodos() = [
        newTodo "Learn F#"
        { newTodo "Learn Elmish" with Completed = true }
    ]

    let init () =
        { Todos = initTodos() }, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | AddNewTodo txt ->
            { state with Todos = (newTodo txt)::state.Todos }, Cmd.none

        | DeleteTodo todoId ->
            state.Todos
            |> List.filter (fun todo -> todo.Id <> todoId)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | ToggleCompleted todoId ->
            state.Todos
            |> List.map
                (fun todo ->
                    if todo.Id = todoId then
                        let completed = not todo.Completed
                        { todo with
                            Completed = completed }
                    else
                        todo)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | StartEditingTodo todoId ->
            state.Todos |> List.map (fun t ->
                match t.Editing with
                | _ when t.Id = todoId -> { t with Editing = Some t.Description }
                | Some _ -> { t with Editing = None }
                | _ -> t)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | CancelEdit ->
            state.Todos |> List.map (fun t ->
                if Option.isSome t.Editing
                then { t with Editing = None }
                else t)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | ApplyEdit txt ->
            state.Todos |> List.map (fun t ->
                match t.Editing with
                | Some _ ->
                    { t with Description = txt; Editing = None }
                | None -> t)
            |> fun todos -> { state with Todos = todos }, Cmd.none


module Util =
    open Elmish

    let onEnterOrEscape dispatchOnEnter dispatchOnEscape (ev: KeyboardEvent) =
        let el = ev.target :?> HTMLInputElement

        match ev.key with
        | "Enter" ->
            dispatchOnEnter el.value
            el.value <- ""
        | "Escape" ->
            dispatchOnEscape()
            el.value <- ""
            el.blur ()
        | _ -> ()

    [<ReactComponent>]
    let Div (classes: string seq) (children: ReactElement seq) =
        Html.div [
            prop.classes classes
            prop.children children
        ]

    [<ReactComponent>]
    let InputField (dispatch: Msg -> unit) =
        let inputRef = React.useRef<HTMLInputElement option>(None)
        Div [ "field"; "has-addons" ] [
           Div [ "control"; "is-expanded" ] [
                Html.input [
                    prop.ref inputRef
                    prop.classes [ "input"; "is-medium" ]
                    prop.autoFocus true
                    prop.onKeyUp (onEnterOrEscape (AddNewTodo >> dispatch) ignore)
                ]
            ]

           Div [ "control" ] [
                Html.button [
                    prop.classes [
                        "button"
                        "is-primary"
                        "is-medium"
                    ]
                    prop.onClick (fun _ ->
                        let txt = inputRef.current.Value.value
                        inputRef.current.Value.value <- ""
                        txt |> AddNewTodo |> dispatch)
                    prop.children [
                        Html.i [
                            prop.classes [ "fa"; "fa-plus" ]
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    let Button isVisible dispatch classes (iconClasses: string list) =
        Html.button [
            prop.type' "button"
            prop.classes (List.choose (fun (s, b) -> if b then Some s else None) [
                "button", true
                "is-invisible", not(isVisible())
                yield! classes
            ])
            prop.style [
                style.marginRight(length.px 4)
            ]
            prop.onClick (fun _ -> dispatch())
            prop.children [
                Html.i [
                    prop.classes iconClasses
                ]
            ]
        ]

    [<ReactComponent>]
    let TodoView (todo: Todo) dispatch =
        let inputRef = React.useRef<HTMLInputElement option>(None)
        let isEditing() = Option.isSome todo.Editing
        let isNotEditing() = Option.isNone todo.Editing

        React.useEffect(fun () ->
            if isEditing() then
                inputRef.current.Value.select()
                inputRef.current.Value.focus())

        Html.li [
            prop.className "box"

            prop.children [
                Div [ "columns" ] [
                    Div [ "column"; "is-7" ] [
                        match todo.Editing with
                        | Some editing ->
                            Html.input [
                                prop.ref inputRef
                                prop.classes [ "input"; "is-medium" ]
                                prop.value editing
                                prop.onKeyUp (onEnterOrEscape (ApplyEdit >> dispatch) (fun _ -> dispatch CancelEdit))
                                prop.onBlur (fun _ -> dispatch CancelEdit)
                            ]
                        | None ->
                            Html.p [
                                prop.className "subtitle"
                                prop.onDoubleClick (fun _ -> dispatch (StartEditingTodo todo.Id))
                                prop.style [
                                    style.userSelect.none
                                    style.cursor.pointer
                                ]
                                prop.children [
                                    Html.text todo.Description
                                ]
                            ]
                    ]

                    Div [ "column"; "is-4" ] [
                        Button isEditing (fun () -> ApplyEdit inputRef.current.Value.value |> dispatch)
                            [ "is-primary", true ]
                            [ "fa"; "fa-save" ]

                        Button isNotEditing (fun () -> ToggleCompleted todo.Id |> dispatch)
                            [ "is-success", todo.Completed ]
                            [ "fa"; "fa-check" ]

                        Button isNotEditing (fun () -> StartEditingTodo todo.Id |> dispatch)
                            [ "is-primary", true ]
                            [ "fa"; "fa-edit" ]

                        Button isNotEditing (fun () -> DeleteTodo todo.Id |> dispatch)
                            [ "is-danger", true ]
                            [ "fa"; "fa-times" ]
                    ]
                ]
            ]
        ]

open Elmish
open Util
open Fable.Core

[<Erase>]
type TodoMVC =
    [<ReactComponent>]
    static member App() =
        let model, dispatch = React.useElmish(init, update)

        React.fragment [
            Html.p [
                prop.className "title"
                prop.children [
                    Html.text "Elmish.Solid To-Do List"
                ]
            ]

            InputField dispatch

            Html.ul (
                model.Todos |> List.map (fun todo -> TodoView todo dispatch)
            )
        ]
