namespace Framework
module Setup =
    open canopy.parallell.functions
    open canopy

    type BrowserConfig = {
        browserType: types.BrowserStartMode;
        size: types.direction;
        home: string;
        compareTimeout: double;
        pageTimeout: double;
        chromeLocation: string;
    }

    let setupBrowser config =
        configuration.compareTimeout <- config.compareTimeout
        configuration.chromeDir <- config.chromeLocation
        configuration.pageTimeout <- config.pageTimeout
        configuration.optimizeBySkippingIFrameCheck <- true
        configuration.elementTimeout <- 200.0

        start config.browserType
        |> fun browser -> 
            try
                pin config.size browser
                url config.home browser
                waitForElement (css "body") browser
            with
            | :? OpenQA.Selenium.WebDriverException -> browser.Quit()        
            
            browser