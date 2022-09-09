namespace Feliz.JSX.React

open System
open Browser.Types
open Fable.Core
open Fable.React

[<AutoOpen>]
module Helpers =
    type Attr with
        static member inline key(value: string): JSX.Prop = "key", value
        static member inline key(value: int): JSX.Prop = "key", value
        static member inline key(value: Guid): JSX.Prop = "key", string value

        static member inline ref<'El when 'El :> Element>(r: IRefValue<'El option>): JSX.Prop = "ref", r
        /// React will call the ref callback with the DOM element when the component mounts, and call it with null when it unmounts.
        static member inline ref<'El when 'El :> Element>(f: 'El option -> unit): JSX.Prop = "ref", f

    let inline toReact (jsxEl: JSX.Element): ReactElement = unbox jsxEl