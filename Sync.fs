namespace Framework

open canopy
open canopy.configuration
open canopy.parallell.functions
open OpenQA.Selenium
open canopy.types

module Sync =
    let mutable browserLocal: IWebDriver = null

    type DriverState =
    | Ready
    | TryAgain
    | ActionRecieved
    | Search
    | Finished

    type DriverEvent<'a> =
        | Action of ('a -> DriverState) * 'a
        | FindNewElement of 'a
        | ElementNotExist of 'a

    let (|Element|NotValidSelector|ElementNotFound|) selector =
        try
            match box selector with
            | :? IWebElement as ele -> Element ele
            | :? string as sel -> Element(element sel browserLocal)
            | _ -> NotValidSelector
        with
        | :? CanopyElementNotFoundException -> ElementNotFound

    let action f ele =
        match ele with
        | Element x ->
            f x browserLocal
            ActionRecieved
        | NotValidSelector -> raise (CanopyNotStringOrElementException "not a valid selector")
        | ElementNotFound -> TryAgain

    let searchForElement ele =
        match ele with
        | Element _ -> Finished
        | NotValidSelector -> raise (CanopyNotStringOrElementException "not a valid selector")
        | ElementNotFound -> Search

    let (|Exists|NotExists|) ele =
        match ele with
        | Element _ -> Exists ele
        | NotValidSelector -> raise (CanopyNotStringOrElementException "not a valid selector")
        | ElementNotFound -> NotExists ele

    let fsm state event =
        let pair = (state, event)

        match pair with
        | (Ready, Action (f, x))
        | (TryAgain, Action (f, x))
        | (ActionRecieved, Action (f, x)) -> f x

        | (Ready, FindNewElement y)
        | (ActionRecieved, FindNewElement y)
        | (Search, FindNewElement y) ->
            match y with
            | Exists _ -> Finished
            | NotExists _ -> Search

        | (ActionRecieved, ElementNotExist x)
        | (Search, ElementNotExist x) ->
            match x with
            | Exists y -> Search
            | NotExists _ -> Finished

        | _ -> state

    let folder state events =
        let timer = System.Diagnostics.Stopwatch()
        timer.Start()

        let rec _looper state events =
            if timer.Elapsed.Seconds > (pageTimeout |> int) then
                CanopyException "Failed to find result of click element"
                |> raise

            match events with
            | [] -> state
            | (x :: y) ->
                fsm state x
                |> fun curState ->
                    match curState with
                    | Search
                    | TryAgain -> _looper curState (x :: y)
                    | _ -> _looper curState y

        _looper state events

    let syncClick browser xEle yEle =
        browserLocal <- browser

        [ Action(action click, xEle)
          FindNewElement yEle ]
        |> folder Ready
        |> ignore

    let syncRightClick browser xEle yEle =
        browserLocal <- browser

        [ Action(action rightClick, xEle)
          FindNewElement yEle ]
        |> folder Ready
        |> ignore
