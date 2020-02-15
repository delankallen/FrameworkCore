namespace Framework
module Setup =
    open canopy.parallell.functions
    open canopy

    type BrowserConfig = {
        browserType: types.BrowserStartMode;
        size: types.direction;
        home: string;
        timeout: double;
        chromeLocation: string;
    }

    let setupBrowser config =
        configuration.compareTimeout <- config.timeout
        configuration.chromeDir <- config.chromeLocation
        configuration.pageTimeout <- config.timeout
        configuration.optimizeBySkippingIFrameCheck <- true

        let browser = start config.browserType
        
        pin config.size browser
        url config.home browser        
        waitForElement (css "body") browser
        browser