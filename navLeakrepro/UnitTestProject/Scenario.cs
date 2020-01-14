using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject
{
    [TestClass]
    public class Scenario : NavLeakReproSession
    {

        const int MAX_NAVIGATIONS = 800;
        const int WAIT_TIMEOUT_SECS = 600; // 10 mn
        const int POLLING_INTERVAL_SECS = 1;

        [TestMethod]
        public void RunNavigationTest()
        {
            // note: inspired from https://github.com/Microsoft/WinAppDriver/issues/364#issuecomment-371291769

            var wait = new DefaultWait<WindowsDriver<WindowsElement>>(session)
            {
                Timeout = TimeSpan.FromSeconds(WAIT_TIMEOUT_SECS),
                PollingInterval = TimeSpan.FromSeconds(POLLING_INTERVAL_SECS)
            };
            wait.IgnoreExceptionTypes(typeof(WebDriverException));

            WindowsElement button;

            // Run MAX_NAVIGATIONS between MainPage and Page2
            for (int i = 0; i < MAX_NAVIGATIONS; i++)  
            {

                button = null;

                // wait for MainPageNavButton button of MainPage to be accessible

                wait.Until(driver =>
                {
                    
                    button = driver.FindElementByAccessibilityId("MainPageNavButton");

                    return button != null;
                });

                if (button!=null)
                {                    
                    session.FindElementByAccessibilityId("MainPageNavButton").Click();
                }
                
                // wait for Page2NavButton button of Page2 to be accessible

                button = null;

                wait.Until(driver =>
                {
                   
                    button = driver.FindElementByAccessibilityId("Page2NavButton");

                    return button != null;
                });

                if (button != null)
                {
                    session.FindElementByAccessibilityId("Page2NavButton").Click();
                }


            }

        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Create session to launch a Repro app window
            Setup(context);

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TearDown();
        }

       
    }

}
