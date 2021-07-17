namespace Framework

module Rest =
    open HttpFs.Client
    open Hopac.Hopac

    type RestQuery = { qName: string; qValue: string }

    let queryHeader (queries: List<RestQuery>) (r: Request) =
        queries
        |> List.map (fun x -> Request.queryStringItem x.qName x.qValue)
        |> List.fold (|>) r

    let sendRequestAsync (request: Request) =
        run
        <| job {
            use! response = getResponse request
            printfn "response code: %d" response.statusCode
            let! bodyStr = Response.readBodyAsString response
            return (response.statusCode, bodyStr)
           }

    let createJob (request: Request) =
        job {
            try
                use! response = getResponse request
                printfn "response code: %d" response.statusCode
            with
            | _ -> ()
        }

    let urlHeader (urlExt:string) = Request.createUrl Get $"{urlExt}"

    let contentHeader app conType (r: Request) =
        r
        |> Request.setHeader (ContentType(ContentType.create (app, conType)))

    let getRequest (queries: List<RestQuery>) =
        urlHeader
        >> queryHeader queries
        >> contentHeader "application" "json"
        >> sendRequestAsync
