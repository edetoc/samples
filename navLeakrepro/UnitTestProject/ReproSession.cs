using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;

namespace UnitTestProject
{
  
    public class NavLeakReproSession
    {
        // Note: append /wd/hub to the URL if you're directing the test at Appium
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";


        //private const string CalculatorAppId = "Microsoft.WindowsCalculator_8wekyb3d8bbwe!App";
        private const string ReproAppId = "8fe903c9-5fd2-46d6-ba25-7188a94e5978_xs3xqzsq8fwcr!App";

        protected static WindowsDriver<WindowsElement> session;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            // Launch  application if it is not yet launched
            if (session == null)
            {
               
                var appiumOptions = new AppiumOptions();
                
                appiumOptions.AddAdditionalCapability("app", ReproAppId);
                appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appiumOptions);

                //// Set implicit timeout to 1.5 seconds to make element search to retry every 500 ms for at most three times
                //session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1.5);
            }
        }

        public static void TearDown()
        {
            // Close the application and delete the session
            if (session != null)
            {
                //session.Quit();   // quit causes the target app to close
                //session = null;
            }
        }

       
    }
}
