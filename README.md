# Feliz.JSX

Use a [Feliz-like](https://zaid-ajaj.github.io/Feliz/) HTML API to compile your F#/Fable code to [JSX](https://reactjs.org/docs/introducing-jsx.html) for [React](https://reactjs.org/) or [Solid](https://www.solidjs.com/) apps.

> **Note**: JSX elements need to be **solved at compile time** so it's not possible to use list generators for attributes.

To test the sample React app run `npm install && npm start`.

When updating a package, edit the RELEASE_NOTES.md of the corresponding project and run `dotnet fsi build.fsx publish` to publish a new version.

> Note: Requires Fable 4 (in beta at the time of writing)
