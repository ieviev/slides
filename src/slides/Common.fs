[<AutoOpenAttribute>]
module Common

[<RequireQualifiedAccess>]
type Theme =
    | Black
    | White
    | League
    | Beige
    | Night
    | Serif
    | Simple
    | Solarized
    | Moon
    | Dracula
    | Sky
    | Blood
    | Other of string

    member x.toFilename =
        match x with
        | Theme.Black -> "black"
        | Theme.White -> "white"
        | Theme.League -> "league"
        | Theme.Beige -> "beige"
        | Theme.Night -> "night"
        | Theme.Serif -> "serif"
        | Theme.Simple -> "simple"
        | Theme.Solarized -> "solarized"
        | Theme.Moon -> "moon"
        | Theme.Dracula -> "dracula"
        | Theme.Sky -> "sky"
        | Theme.Blood -> "blood"
        | Theme.Other(s) -> s

[<AutoOpen>]
module Slides =
    type Settings = {
        SourcePath: string
        OutPath: string
        Theme: Theme
        Port: int
    } with
        static member Default = {
            Theme = Theme.Black
            SourcePath = "src"
            OutPath = "public"
            Port = 8080
        }


let getDataDir() =
    let entryAssembly = System.Reflection.Assembly.GetEntryAssembly().Location
    match entryAssembly |> Path.fileName with
    | "fsi.dll" ->
        let typeAssembly = typeof<Slides.Settings>.Assembly.Location
        let p3 =
            System.IO.DirectoryInfo(typeAssembly).Parent.Parent.Parent
        Path.combine(p3.FullName,"content","data")
    | _ ->
        Path.combine(entryAssembly,"data")



open System.IO
let internal copyAssets(settings:Settings) =
    let revealDataPath = Path.combine(getDataDir(),"reveal")
    let outpath = Path.combine($"{settings.OutPath}","reveal")
    let outinfo = DirectoryInfo(outpath)
    match outinfo.Exists with
    | true ->
        ()
        // stdout.WriteLine "assets exist in public/"
    | false ->
        ()
        // stdout.WriteLine "copying assets to public/reveal/"
        revealDataPath |> Directory.copyTo outpath


