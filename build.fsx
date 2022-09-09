#r "nuget: Fable.PublishUtils, 2.4.0"

open PublishUtils

let args =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> List.ofArray

match args with
| IgnoreCase "publish"::_ ->
    pushFableNuget "src/Feliz.JSX.React/Feliz.JSX.React.fsproj" [] doNothing
    pushFableNuget "src/Feliz.JSX.Solid/Feliz.JSX.Solid.fsproj" [] doNothing
| _ -> ()
