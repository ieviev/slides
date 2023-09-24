module Server


open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket

let refreshEvent = Event<unit>()

let ws (webSocket: WebSocket) (context: Suave.Http.HttpContext) =
    socket {
        while true do
            do! SocketOp.ofAsync (Async.AwaitEvent refreshEvent.Publish)

            let msg =
                System.Text.Encoding.ASCII.GetBytes("refresh!")
                |> ByteSegment

            do! webSocket.send Text msg true
    }

open System.IO
open Suave
open Suave.Filters
open Suave.Operators

let app (settings: Settings) : WebPart =
    choose [
        path "/websocket"
        >=> handShake ws
        Writers.setHeader "Cache-Control" "no-cache, no-store, must-revalidate"
        >=> Writers.setHeader "Pragma" "no-cache"
        >=> Writers.setHeader "Expires" "0"
        >=> choose [
            path "/"
            >=> Files.file (Path.combine (settings.OutPath, "index.html"))
            Files.browseHome
            RequestErrors.NOT_FOUND "404 file not found"
        ]
    ]


let noLogger =
    { new Suave.Logging.Logger with
        member this.log
            (arg1: Logging.LogLevel)
            (arg2: Logging.LogLevel -> Logging.Message)
            : unit =
            ()

        member this.logWithAck
            (arg1: Logging.LogLevel)
            (arg2: Logging.LogLevel -> Logging.Message)
            : Async<unit> =
            Async.Sleep 0

        member this.name: string array = [||]
    }

let createConfig (settings: Settings) = {
    defaultConfig with
        logger = noLogger
        homeFolder = Some(Path.GetFullPath(settings.OutPath))
        compressedFilesFolder = Some(Path.GetTempPath())
        bindings = [
            {
                HttpBinding.defaults with
                    socketBinding = {
                        HttpBinding.defaults.socketBinding with
                            port = uint16 settings.Port
                    }
            }
        ]
}
