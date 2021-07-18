# FrameworkCore
Framework core to be used as a basis for an automation framework written in F#. Uses the [canopy](https://github.com/lefthandedgoat/canopy) library for webdriver control.

FrameworkCore is composed of generic functions to be used in writing the actions that drive tests.

## Database

Database contains functions for connecting to a database to directly retrieve and manipulate data.

- `slqConBuilder ()`
- `sqlQuery (queryString:string)`
- `sqlQueryScalar (queryString:string)`
- `sqlNonQuery (queryString:string)`

Database needs to be refactored but is a low priority.

The Rest api calls are the preferred method of retrieving database information. 

## Rest

Basic rest functions where the request is sent and the response received.

`sendRequestAsync (request: Request)`

___


Used for sending a single request. Writes the response code to the console and returns the response as a string. 

`createJob (request: Request)`

___


Used for creating multiple requests to be batched later.
The async library, Hopac, likes run to be called once per program, this allows us to bundle up requests as jobs, which will then be ran later in the program.

## Setup

Setup is responsible for configuring the webdriver.
 
    type BrowserConfig = {
        browserType: types.BrowserStartMode;
        size: types.direction;
        home: string;
        compareTimeout: double;
        pageTimeout: double;
        chromeLocation: string;
    }

`setupBrowser (config:BrowserConfig)`

___


Takes in the `BrowserConfig` type that contains all the information needed for timeouts and Chrome's location.
`setupBrowser` returns the WebDriver. 

## Sync

Sync contains all the functions for ordering actions.
One of the main issues with UI automation is the variability of UI.
Sometimes we don't wait long enough for an element to load.
Sometimes we the action to make the element appear didn't happen.
Functions in Sync are named clearly to their expectations. 

When Selenium can't find an element it throws an exception.
Throwing an exception is fine when the element can not actually be found, but many times something else is preventing the element from being found.

`someClick browser xEle` & `someRightClick browser xEle`

___


The base function of the Sync functions is `someClick`.
`someClick` wraps the exceptions we want to catch and ignore in the F# Option type. (More on [Option types](https://fsharpforfunandprofit.com/posts/the-option-type/))

The function returns `None` if it encounters an exception. It returns `Some xEle` if it finds the element.

`isDisplayed browser ele`

___


Sometimes we just need a simple true false for something displayed.
Selenium has a built in `isDisplayed()`, but it throws an exception when it can't find an element.
This function specifically wraps the `StaleElementReferenceException`, which most often occurs on a page reload or when we have left a page.

`rec clickWhileDisplayed browser ele`

___


Uses the `someClick` function to click an element while it is visible or until WebDriver times out.
This is useful for save buttons of modal windows.

`syncClick browser xEle yEle` & `syncRightClick browser xEle yEle`

___


The `syncClick` functions are generally the best option when we want to click something. 
These functions will make multiple attempts to click the `xEle`, if it makes a successful click it will then search for the `yEle`. If `yEle` is found it returns `yEle`, if `yEle` is not found it starts the process over. 

These functions will loop until WebDriver times out, or until it encounters:

- `ElementClickInterceptedException`
- `StaleElementReferenceException` 
- `ElementNotInteractableException`


`newPageAction (browser : IWebDriver) (action : IWebElement -> IWebDriver -> unit) xEle yEle`

___

This is the beginning of Sync being refactored into a cleaner, more generic state machine.
There will be a main function that takes an action, thus eliminating the need to implement a sync function for every action. For example, `syncClick` and `syncRightClick` are basically the same function, just a different internal action.

## Xpath Query

Xpath Query is a DSL(Domain Specific Language) for building xpath queries in a SQL like syntax. All elements should be found using this.

Xpath queries are strings passed to the webdriver, so they don't benefit from intellisense and syntax checking, which leaves a lot of room for tiny errors and hours of frustration.

The core of the DSL is an `AttributeType` passed through a pattern match.

    type AttributeType =
        | Id of string
        | Class of string
        | Title of string
        | Attribute of string * string
        | Text of string
        | InnerNode of string
        | DataHashLocation of string
        | NodeIndex of int
        | NotContain of AttributeType

Basic example to find the save button of the Workspace Tools Dialog:

    let mainDiv = select "div" |> where (Id "selectWorkspaceToolsDialog")
    let savebtn = select "button" |> where (Class "save-button") |> from (mainDiv)

`DynamicLoadingMaps.fs` contains a more complex example.