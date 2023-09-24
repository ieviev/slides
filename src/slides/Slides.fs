module Slides

[<AutoOpen>]
module internal Internal =
    let internal ensureAssets (settings: Settings) : unit =
        Directory.createIfNotExists "public"
        copyAssets (settings)

        let srcDir =
            System.IO.DirectoryInfo(settings.SourcePath)

        if not srcDir.Exists then
            srcDir.Create()
            [
                "# Slides"
                ""
                "---"
                "- First slide"
                ""
                "---"
                "### Second slide"

            ]
            |> String.concat "\n"

            |> File.writeTo (Path.combine(srcDir.FullName,"index.md"))

        System.IO.Directory.EnumerateDirectories(settings.SourcePath)
        |> Seq.iter (fun v ->
            let dname = Path.fileNameWithoutExtension (v)
            Directory.copyTo (Path.combine (settings.OutPath, dname)) v
        )

        System.IO.Directory.EnumerateFiles(settings.SourcePath)
        |> Seq.map System.IO.FileInfo
        |> Seq.where (fun v ->
            v.Extension
            <> ".md"
        )
        |> Seq.iter (fun v ->
            v.CopyTo(Path.combine (settings.OutPath, v.Name))
            |> ignore
        )


    let internal updateSlides (settings: Settings) =
        Directory.enumerateFiles settings.SourcePath
        |> Seq.tryFind (fun v -> System.IO.Path.GetExtension(v) = ".md")
        |> function
            | None ->
                stdout.WriteLine
                    $"no markdown found in {System.IO.Path.GetFullPath(settings.SourcePath)}"
            | Some mdFile ->
                stdout.WriteLine $"processing {mdFile}"

                let mdDocument =
                    mdFile
                    |> File.readAllText
                    |> FSharp.Formatting.Markdown.Markdown.Parse

                let html = Reveal.fullHtmlFromMarkdownDocument settings mdDocument

                html
                |> File.writeTo (Path.combine (settings.OutPath, "index.html"))


    let internal loadSettings () =
        match File.exists "slides.json" with
        | false -> Settings.Default
        | true ->
            stdout.WriteLine "reading slides.json"

            "slides.json"
            |> File.readAllText
            |> Json.parseWithThoth<Settings>
            |> Result.defaultWith failwith

    let inline internal listenSettings (onChanged) =
        Async.Start(
            async {
                Directory.watch (
                    "slides.json",
                    (fun chg -> onChanged()),
                    workDirectory = "."
                )
            }
        )

    let inline internal listenServer (settings: Settings) : unit =
        let _, start =
            Suave.Web.startWebServerAsync (Server.createConfig (settings)) (Server.app (settings))
        Async.Start(start)


let start(settings:Settings) =
    let mutable settings = settings
    ensureAssets (settings)
    updateSlides (settings)
    listenServer(settings)
    listenSettings(fun v ->
        settings <- loadSettings()
        updateSlides(settings)
    )

    stdout.WriteLine $"showing slides at http://localhost:{settings.Port}/"

    do
        Async.Start(
            async {
                Directory.watch (
                    "*.json",
                    (fun chg ->
                        match System.IO.Path.GetExtension(chg.Name) with
                        | ".json" ->
                            if Path.fileName (chg.Name) = "slides.json" then
                                stdout.WriteLine "loading settings"
                                settings <- loadSettings ()
                                updateSlides (settings)
                            else
                                ()
                        | ext -> stdout.WriteLine $"changed ({ext}): {chg.Name}"
                    ),
                    workDirectory = "."
                )
            }
        )

    Directory.watch (
        "*",
        (fun chg ->
            match System.IO.Path.GetExtension(chg.Name) with
            | ".md" ->
                updateSlides (settings)
                Server.refreshEvent.Trigger()
            | ".png"
            | ".jpg" ->
                stdout.WriteLine "updating images"
                copyAssets (settings)
            | ext -> stdout.WriteLine $"changed ({ext}): {chg.Name}"
        ),
        workDirectory = settings.SourcePath
    )
    ()


// run app
