module TodoMVC

open Feliz
open Feliz.JSX
open Feliz.UseElmish
open Browser.Types
open Fable.Core

module private Elmish =
    open System
    open Elmish

    type Todo =
        {
            Id: Guid
            Description: string
            Completed: bool
        }

    type State = { Todos: Todo list }

    type Msg =
        | AddNewTodo of string
        | DeleteTodo of Guid
        | ToggleCompleted of Guid
        | ApplyEdit of Guid * string

    let newTodo txt =
        {
            Id = Guid.NewGuid()
            Description = txt
            Completed = false
        }

    let initTodos (count: int) =
        [
            newTodo "Learn F#"
            { newTodo $"Learn Elmish  in {count} days" with
                Completed = true
            }
        ]

    let init (count: int) = { Todos = initTodos count }, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | AddNewTodo txt ->
            { state with
                Todos = (newTodo txt) :: state.Todos
            },
            Cmd.none

        | DeleteTodo todoId ->
            state.Todos
            |> List.filter (fun todo -> todo.Id <> todoId)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | ToggleCompleted todoId ->
            state.Todos
            |> List.map (fun todo ->
                if todo.Id = todoId then
                    let completed = not todo.Completed
                    { todo with Completed = completed }
                else
                    todo)
            |> fun todos -> { state with Todos = todos }, Cmd.none

        | ApplyEdit (todoId, txt) ->
            state.Todos
            |> List.map (fun todo ->
                if todo.Id = todoId then
                    { todo with Description = txt }
                else
                    todo)
            |> fun todos -> { state with Todos = todos }, Cmd.none

module private Util =
    open Elmish

    let inline toJsx (el: ReactElement) : JSX.Element = unbox el
    let inline toStyle (styles: IStyleAttribute list) : obj = JsInterop.createObj (unbox styles)

    let onEnterOrEscape dispatchOnEnter dispatchOnEscape (ev: KeyboardEvent) =
        let el = ev.target :?> HTMLInputElement

        match ev.key with
        | "Enter" ->
            dispatchOnEnter el.value
            el.value <- ""
        | "Escape" ->
            dispatchOnEscape ()
            el.value <- ""
            el.blur ()
        | _ -> ()

    [<JSX.Component>]
    let InputField (dispatch: Msg -> unit) =
        let inputRef = React.useRef<HTMLInputElement option> (None)

        JSX.jsx
            $"""
        <div className="field has-addons">
            <div className="control is-expanded">
                <input ref={inputRef}
                       className="input is-medium"
                       autoFocus={true}
                       onKeyUp={onEnterOrEscape (AddNewTodo >> dispatch) ignore}>
                </input>
            </div>
            <div className="control">
                <button className="button is-primary is-medium"
                    onClick={fun _ ->
                                 let txt = inputRef.current.Value.value
                                 inputRef.current.Value.value <- ""
                                 txt |> AddNewTodo |> dispatch}>
                    <i className="fa fa-plus"></i>
                </button>
            </div>
        </div>
        """

    [<JSX.Component>]
    let Button isVisible (iconClass: string) (classes: (string * bool) list) dispatch =
        JSX.jsx
            $"""
        <button type="button"
            className=
            {String.concat
                 " "
                 [
                     "button"
                     if not isVisible then
                         "is-invisible"
                     for class', b in classes do
                         if b then
                             class'
                 ]}
            style=
            {toStyle [ style.marginRight (length.px 4); style.marginLeft (length.px 4) ]}
            onClick={fun _ -> dispatch ()}
        >
            <i className={iconClass}></i>
        </button>
        """

    [<JSX.Component>]
    let TodoView dispatch (todo: Todo) =
        let inputRef = React.useRef<HTMLInputElement option> (None)
        let edit, setEdit = React.useState<string option> (None)
        let isEditing = Option.isSome edit
        let isNotEditing = Option.isNone edit

        let applyEdit edit =
            ApplyEdit(todo.Id, edit) |> dispatch
            setEdit None

        React.useEffect (
            (fun () ->
                if isEditing then
                    inputRef.current.Value.select ()
                    inputRef.current.Value.focus ()

                None),
            [| box isEditing |]
        )

        JSX.jsx
            $"""
        <li className="box">
            <div className="columns">
                <div className="column is-7">
                {match edit with
                 | Some edit ->
                     // Feliz code
                     Html.input
                         [
                             prop.ref inputRef
                             prop.classes [ "input"; "is-medium" ]
                             prop.value edit
                             prop.onChange (Some >> setEdit)
                             prop.onKeyDown (onEnterOrEscape applyEdit (fun _ -> setEdit None))
                             prop.onBlur (fun _ -> setEdit None)
                         ]
                 | None ->
                     Html.p
                         [
                             prop.className "subtitle"
                             prop.onDoubleClick (fun _ -> Some todo.Description |> setEdit)
                             prop.style [ style.userSelect.none; style.cursor.pointer ]
                             prop.children [ Html.text todo.Description ]
                         ]}
                </div>
                <div className="column is-5">
                    {Button isEditing "fa fa-save" [ "is-primary", true ] (fun () -> applyEdit edit.Value)}

                    {Button isNotEditing "fa fa-check" [ "is-success", todo.Completed ] (fun () -> ToggleCompleted todo.Id |> dispatch)}

                    {Button isNotEditing "fa fa-edit" [ "is-primary", true ] (fun () -> Some todo.Description |> setEdit)}

                    {Button isNotEditing "fa fa-times" [ "is-danger", true ] (fun () -> DeleteTodo todo.Id |> dispatch)}
                </div>
            </div>
        </li>
        """

open Elmish
open Util

[<JSX.Component>]
let App () =
    let model, dispatch = React.useElmish (init, update, arg = 2)

    JSX.jsx
        $"""
    <>
        <p className="title">Elmish.React To-Do List</p>
        {InputField dispatch}
        {model.Todos |> List.map (TodoView dispatch)}
    </>
    """
