module Reveal

open Feliz.ViewEngine
open FSharp.Formatting.Markdown

let renderFullHtml (settings: Settings) (children: ReactElement seq) =
    Html.html [
        Html.head [
            Html.rawText """<meta charset="utf-8">"""
            Html.rawText "<title>slides</title>"
            Html.link [
                prop.rel "stylesheet"
                prop.href "reveal/reveal.css"
            ]
            Html.link [
                prop.rel "stylesheet"
                prop.href $"reveal/theme/%s{settings.Theme.toFilename}.css"
            ]
        ]
        Html.body [
            Html.div [
                prop.className "reveal"
                prop.children [
                    Html.div [
                        prop.className "slides"
                        prop.children children
                    ]
                ]
            ]
            Html.script [ prop.src "reveal/reveal.js" ]
            Html.link [
                prop.rel "stylesheet"
                prop.href "reveal/plugin/highlight/style.css"
            ]
            Html.script [ prop.src "reveal/plugin/highlight/highlight.js" ]
            Html.script [ prop.src "reveal/plugin/math/math.js" ]
            Html.script [
                prop.custom ("language", "javascript")
                prop.custom ("type", "text/javascript")
                prop.dangerouslySetInnerHTML
                    """
			Reveal.initialize({
				plugins: [ RevealHighlight, RevealMath.KaTeX ],
				hash:true,
				pdfSeparateFragments: false,
				transition:"none",
				controls:false
			});
		"""
            ]
            Html.script [
                prop.custom ("language", "javascript")
                prop.custom ("type", "text/javascript")
                prop.dangerouslySetInnerHTML
                    """
                        if (window.location.hostname === "localhost") {
                            websocket = new WebSocket("ws://" + window.location.host + "/websocket");
                            websocket.onmessage = function (evt) {
                                window.location.reload();
                            };
                        }
                    """
            ]
        ]
    ]
    |> Render.htmlView


let fullHtmlFromMarkdownDocument (settings: Settings) (mdoc: MarkdownDocument) =

    let slides =
        mdoc
        |> Markdown.splitIntoSlides

    match slides with
    | title :: content ->
        renderFullHtml settings [
            Markdown.createTitleSlide mdoc.DefinedLinks title
            yield!
                content
                |> List.map (Markdown.createContentSlide (mdoc.DefinedLinks))
        ]
    | _ -> renderFullHtml settings []
