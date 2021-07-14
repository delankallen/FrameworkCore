namespace Framework

module Sync =
    open canopy.parallell.functions
    open canopy
    open OpenQA.Selenium
    
    let private wrapEle action ele =
            match ele |> Option.map action with
            | Some _ -> ele
            | None -> None    
    
    let private isStale (xEle:IWebElement) =
        try
            not(xEle.Displayed)
        with
        | :? StaleElementReferenceException -> true
        | :? WebDriverException -> false

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
            sleep 2
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

    let loginFun browser loginButton =
        let mutable i = 0
        someClick browser loginButton
        |> (fun x -> 
            while not(isStale x.Value) do
                i <- i + 1
                sleep 3
        )

    type ExpectedResult = 
        | NewPage
        | NewBlockingElement
        | NewElement
    
    type ElementState =
        | ElementStale
        | ElementBlocked
        | ElementNotInteractable
        | ElementNotVisable
        | ElementNotFound
        | ElementGood

    type NewElement = {
        WebElement: IWebElement;
        State: ElementState;
    }

    let someElement browser ele = 
        match box ele with
        | :? IWebElement as element -> Some element
        | :? string as selector -> someElement selector browser
        | _ -> raise (types.CanopyNotStringOrElementException(sprintf "Can't click %O because it is not a string or WebElement" ele))

    let private _action (browser : IWebDriver) (action : IWebElement -> IWebDriver -> unit) xEle =
        try
            action xEle browser
            ElementGood
        with
        | :? ElementClickInterceptedException -> ElementBlocked
        | :? StaleElementReferenceException -> ElementStale
        | :? ElementNotInteractableException -> ElementNotInteractable

    let rec searchForElement browser ele = 
        match someElement browser ele with
        | Some element -> element
        | None -> 
            sleep 1
            searchForElement browser ele

    let waitForElement2 browser ele =
        let timer = System.Diagnostics.Stopwatch();
        timer.Start();

        let rec _search browser ele =
            if timer.Elapsed.Seconds > (configuration.pageTimeout |> int) then 
                types.CanopyException "Failed to find result of click element" |> raise
            match someElement browser ele with
            | Some element -> element
            | None -> 
                sleep 1
                _search browser ele
        _search browser ele

    let newPageAction (browser : IWebDriver) (action : IWebElement -> IWebDriver -> unit) xEle yEle =
        let timer = System.Diagnostics.Stopwatch();
        timer.Start();
        let rec _things (browser : IWebDriver) (action : IWebElement -> IWebDriver -> unit) xEle yEle =
            if timer.Elapsed.Seconds > (configuration.pageTimeout |> int) then 
                types.CanopyException "Failed to find result of click element" |> raise
                
            match _action browser action xEle with
            | ElementGood -> 
                    try
                        waitForElement yEle browser
                        element yEle browser
                    with
                    | :? WebDriverTimeoutException -> _things browser action xEle yEle
            | _ -> searchForElement browser yEle
        _things browser action xEle yEle