namespace Framework

module Sync =
    open canopy.parallell.functions
    open canopy
    open FSharpPlus
    open OpenQA.Selenium
    
    let wrapEle action ele =
            match ele |> Option.map action with
            | Some _ -> ele
            | None -> None
    
    let someClick browser xEle =
        try
            match box xEle with
            | :? IWebElement as element ->
                    click element browser
                    Some element
            | :? string as selector ->
                    someElement selector browser
                    |> Option.map (fun x ->
                        click x browser
                        x)
            | _ -> raise (types.CanopyNotStringOrElementException(sprintf "Can't click %O because it is not a string or webelement" xEle))
        with
        | :? ElementClickInterceptedException -> None
        | :? StaleElementReferenceException -> None
        | :? ElementNotInteractableException -> None

    let someRightClick browser xEle =
        try
            match box xEle with
            | :? IWebElement as element ->
                    rightClick element browser
                    Some element
            | :? string as selector ->
                    someElement selector browser
                    |> Option.map (fun x ->
                        rightClick x browser
                        x)
            | _ -> raise (types.CanopyNotStringOrElementException(sprintf "Can't click %O because it is not a string or webelement" xEle))
        with
        | :? ElementClickInterceptedException -> None
        | :? StaleElementReferenceException -> None
        | :? ElementNotInteractableException -> None

    let isDisplayed item browser =         
        try
            match box item with
            | :? IWebElement as element -> Some element.Displayed
            | :? string as selector ->
                match someElement selector browser with
                | Some a -> Some a.Displayed
                | None -> None
            | _ -> raise (types.CanopyNotStringOrElementException(sprintf "Can't click %O because it is not a string or webelement" item))
        with
        | :? StaleElementReferenceException -> None

    let rec clickWhileDisplayed browser ele =
        match ele |> someClick browser with
        | Some _ ->
            sleep 1
            ele |> clickWhileDisplayed browser
        | None -> ()
        
    let syncClick browser xEle yEle =
        let mutable i = 0
        let mutable clicked = false
        let rec searchState () =
            match someElement yEle browser with
            | Some a -> a
            | None ->
                i <- i + 1
                match i with
                | x when x >= (configuration.pageTimeout |> int) -> types.CanopyException "Failed to find result of click element" |> raise
                | _ ->
                    sleep 1
                    searchState ()
                    
        let rec beginningState () =
            match someElement yEle browser with
            | Some a -> a
            | None ->
                match someClick browser xEle with
                | Some _ ->
                    sleep 2
                    clicked <- true
                    beginningState ()
                | None _ -> 
                    if clicked then
                        searchState()
                    else
                        beginningState()

        beginningState ()

    let syncRightClick browser xEle yEle =
        let mutable i = 0
        let mutable clicked = false
        let rec searchState () =
            match someElement yEle browser with
            | Some a -> a
            | None ->
                i <- i + 1
                match i with
                | x when x >= (configuration.pageTimeout |> int) -> types.CanopyException "Failed to find result of click element" |> raise
                | _ ->
                    sleep 1
                    searchState ()
                    
        let rec beginningState () =
            match someElement yEle browser with
            | Some a -> a
            | None ->
                match someRightClick browser xEle with
                | Some _ ->
                    sleep 2
                    clicked <- true
                    beginningState ()
                | None _ -> 
                    if clicked then
                        searchState()
                    else
                        beginningState()

        beginningState ()