using LogManagement;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace HorribleSubs
{
    public class WebDriver : IDisposable
    {

        private IWebDriver driver = null;
        public string Downloads { get; private set; }
        public WebDriver(string downloadFolderName = "Torrents")
        {

            var options = new ChromeOptions();
            if (!Directory.Exists("ChromeProfile"))
            {
                Directory.CreateDirectory("ChromeProfile");
            }
            Downloads = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{downloadFolderName}";
            options.AddArgument($"--user-data-dir={Environment.CurrentDirectory}{Path.DirectorySeparatorChar}ChromeProfile");
            options.AddUserProfilePreference("download.default_directory", Downloads);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            driver = new ChromeDriver(options);
        }

        public WebDriver Go(string location)
        {
            driver.Navigate().GoToUrl(location);
            return this;
        }

        public WebDriver WaitForTitle(string title)
        {
            return WaitForTitle(title, TimeSpan.FromSeconds(10));
        }
        public WebDriver WaitForTitle(string title, TimeSpan timeSpan)
        {
            var wait = new WebDriverWait(driver, timeSpan);
            wait.Until(d => d.Title.StartsWith(title, StringComparison.OrdinalIgnoreCase));
            return this;
        }
        
        public IWebElement GetBy(string id="", string name ="", string xPath = "")
        {
            try
            {
                IWebElement _element = null;
                if (!string.IsNullOrEmpty(id))
                {
                    _element = driver.FindElement(By.Id(id));
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    _element = driver.FindElement(By.Name(name));
                }
                else if (!string.IsNullOrEmpty(xPath))
                {
                    _element = driver.FindElement(By.XPath(xPath));
                }
                return _element;
            }
            catch(Exception ex)
            {
                Log.Write($"Unable to get element by :{id ?? name ?? xPath}", LogManagement.LogLevel.Error);
                Log.Exception(ex);
                return null;
            }
        }

        public ReadOnlyCollection<IWebElement> GetAllByXPath(string xPath)
        {
            try
            {
                var _collection = driver.FindElements(By.XPath(xPath));
                return _collection;
            }
            catch(Exception ex)
            {
                Log.Write($"Unable to get element by :{xPath}", LogManagement.LogLevel.Error);
                Log.Exception(ex);
                return null;
            }
        }

        public IWebElement WaitForBy(string id="", string name="", string xPath = "")
        {
            return WaitForBy(TimeSpan.FromSeconds(10), id, name, xPath);
        }
        public IWebElement WaitForBy(TimeSpan timeSpan, string id = "", string name = "", string xPath = "")
        {
            var wait = new WebDriverWait(driver, timeSpan);
            try
            {
                wait.Until(e => GetBy(id, name, xPath) != null);
                return GetBy(id, name, xPath);
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public bool Click(IWebElement element)
        {
            try
            {
                element.Click();
                return true;
            }
            catch(Exception ex)
            {
                if (element != null)
                {
                    Log.Write($"Unable to click on element :{element.XPath()}", LogManagement.LogLevel.Error);
                }
                Log.Exception(ex);
                return false;
            }
        }

        public bool Click(string id="", string name="", string xPath = "")
        {
            try
            {
                GetBy(id, name, xPath).Click();
                return true;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch(Exception ex)
            {
                Log.Write($"Unable to click on element :{id ?? name ?? xPath}", LogManagement.LogLevel.Error);
                Log.Exception(ex);
                return false;
            }
        }

        public WebDriver WaitUntilClicked(string id = "", string name = "", string xPath = "")
        {
            return WaitUntilClicked(TimeSpan.FromSeconds(1), id, name, xPath);
        }
        public WebDriver WaitUntilClicked(TimeSpan timeSpan ,string id= "", string name="", string xPath = "")
        {
            var wait = new WebDriverWait(driver, timeSpan);
            wait.Until(e => Click(id, name, xPath) == true);
            return this;
        }

        public WebDriver WaitUntilClicked(IWebElement element)
        {
            return WaitUntilClicked(element, TimeSpan.FromSeconds(10));
        }
        public WebDriver WaitUntilClicked(IWebElement element, TimeSpan timeSpan)
        {
            var wait = new WebDriverWait(driver, timeSpan);
            wait.Until(e => Click(element) == true);
            return this;
        }

        public WebDriver SendKeys(IWebElement element , string keys)
        {
            element.SendKeys(keys);
            return this;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    driver.Close();
                    driver.Quit();
                    driver.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Downloads = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~WebDriver()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
