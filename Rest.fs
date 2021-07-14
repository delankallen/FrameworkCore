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
            return bodyStr
        }

    let createJob (request: Request) = job {
        try        
            use! response = getResponse request
            printfn "response code: %d" response.statusCode
        with
        | _ -> ()
    }
