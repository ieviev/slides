module Markdown

open FSharp.Formatting.Markdown
open System.IO
open Feliz.ViewEngine

module List =
    let splitBy fn data =
        (([], []),data)
        ||> Seq.fold
            (fun (curr, all) v ->
                match fn v with
                | true -> [], List.rev curr :: all
                | _ -> v :: curr, all
            )
        |> (fun (curr, all) ->
            List.rev (List.rev curr :: all)
        )


let createContentSlide (links) (mdParagraphs:MarkdownParagraph list) =
    let doc = MarkdownDocument(mdParagraphs, links)
    Html.section [
        prop.className "content"
        prop.dangerouslySetInnerHTML (Markdown.ToHtml(doc))
    ]

let createTitleSlide (links) (mdParagraphs:MarkdownParagraph list) =
    let doc = MarkdownDocument(mdParagraphs, links)
    Html.section [
        prop.className "title"
        prop.dangerouslySetInnerHTML (Markdown.ToHtml(doc))
    ]

let splitIntoSlides (mdoc:MarkdownDocument) =
    mdoc.Paragraphs
    |> List.splitBy (
        function
        | HorizontalRule(_) -> true
        | _ -> false
    )

let processDocument (doc:MarkdownDocument) =
    let paragraphs = doc.Paragraphs
    stdout.WriteLine $"%A{paragraphs}"


let processFile(filePath:string) =
    let doc = Markdown.Parse(File.ReadAllText(filePath))
    processDocument doc
    stdout.WriteLine $"%A{doc}"






