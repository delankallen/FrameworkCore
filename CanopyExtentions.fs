namespace Framework
module CanopyExtensions =
    open canopy.parallell.functions
    open OpenQA.Selenium
    open canopy
    open Framework.XpathQuery

    let sendKeys selector text browser =
        match box selector with
        | :? IWebElement as element -> element.SendKeys(text)
        | :? string as cssSelector -> (element cssSelector browser).SendKeys(text)
        | _ -> raise (types.CanopyNotStringOrElementException(sprintf "Can't sendkeys to %O because it is not a string or webElement" selector))

    let private _href value = sprintf "[href = '%s']" value
    let href cssSelector = _href cssSelector |> css

    let private _name value = sprintf "*[name = '%s']" value
    let name cssSelector = _name cssSelector |> css

    let exists browser selector =
        match someElement selector browser with
        | Some(_) -> true
        | None -> false

    let jsClick browser locator =
        try
            locator |> xPathForJs |> sprintf "%s.click()" |> js <| browser |> Some
        with
        | _ -> None

    let jsClickWhile browser ele =
        let mutable i = 0
        let rec clickWhile () =
            match ele |> jsClick browser with
            | Some _ ->
                i <- i + 1
                match i with
                | x when x >= (configuration.pageTimeout |> int) -> types.CanopyException "Element never went away" |> raise
                | _ ->
                    sleep 1
                    clickWhile ()
            | None -> ()

        clickWhile()