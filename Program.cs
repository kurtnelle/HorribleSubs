using LogManagement;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace HorribleSubs
{
    class Program
    {

        static DateTime startedTime = DateTime.Now;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Log.LogFileName = Log.DefaultLogFileName;
            Log.LogEvent += Log_LogEvent;
            Log.Informational($"Started at {startedTime}");
            if (args.Length == 1)
            {
                DoWork(args[0].Trim());
            }
            else
            {
                Console.Write("Anime Title? ");
                var _title = Console.ReadLine().Trim();
                if (!string.IsNullOrEmpty(_title))
                {
                    DoWork(_title);
                }
            }
            Environment.Exit(0);
        }


        public static void DoWork(string animeName)
        {

            if (string.IsNullOrEmpty(animeName))
            {
                return;
            }
            using var _webDriver = new WebDriver();
            var _searchBox = _webDriver.Go("https://horriblesubs.info/")
                .WaitForTitle("HorribleSubs")
                .WaitForBy("", "", "//node()[@class='latest-search-bar form-control']");
            Log.Informational($"Searching for {animeName}");
            var _searchResult = _webDriver.SendKeys(_searchBox, animeName + Keys.Return)
                .WaitForBy("", "", $"//div[@class='latest-releases']/ul/li/a[starts-with(text(),'{animeName}')]");
            if (_searchResult != null)
            {
                var _animePage = _webDriver.GetBy("", "", $"//div[@class='latest-releases']/ul/li/a[starts-with(text(),'{animeName}')]")
                    .GetAttribute("href");

                _animePage = _animePage.Split("#")[0];

                _webDriver.Go(_animePage).WaitForTitle(animeName);

                var _showMore = _webDriver.WaitForBy(TimeSpan.FromSeconds(1), "", "", "//node()[@class='more-button']");
                while (_showMore != null)
                {
                    _webDriver.WaitUntilClicked(TimeSpan.FromSeconds(10), "", "", "//node()[@class='more-button']");
                    Thread.Sleep(1000);
                    _showMore = _webDriver.WaitForBy(TimeSpan.FromSeconds(10), "", "", "//node()[@class='more-button']");
                }

                List<string> _1080pUrls = new List<string>();
                foreach (IWebElement _anchorContainer in _webDriver.GetAllByXPath("//node()[@class='rls-info-container']"))
                {
                    var _a = _anchorContainer.FindElement(By.XPath("div[@class='rls-links-container']/div[@class='rls-link link-1080p']/span[@class='dl-type hs-torrent-link']/a"));
                    string _href = _a.GetAttribute("href");
                    _1080pUrls.Add(_href);
                }

                Log.Informational($"{_1080pUrls.Count} torrent links found. Downloading all.");

                using var client = new WebClient();

                var _animeTitle = _webDriver.GetBy("", "", "//h1[@class='entry-title']").Text;

                string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidPathChars()));
                string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

                Regex.Replace(_animeTitle, invalidRegStr, "_");

                string _folderName = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}{_animeTitle}";
                if (!Directory.Exists(_folderName))
                {
                    Directory.CreateDirectory(_folderName);
                }

                Regex _fileNameExpression = new Regex("filename=\"([^\"]+)\"", RegexOptions.Compiled);
                foreach (string _url in _1080pUrls.Reversed())
                {
                    var data = client.DownloadData(_url);
                    string _fileName = "";


                    if (!String.IsNullOrEmpty(client.ResponseHeaders["Content-Disposition"]))
                    {
                        var _contentDisposition = HttpUtility.UrlDecode(client.ResponseHeaders["Content-Disposition"]);
                        
                        if(_fileNameExpression.IsMatch(_contentDisposition))
                        {
                            _fileName = _fileNameExpression.Match(_contentDisposition).Groups[1].Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(_fileName))
                    {
                        _fileName = $"{_folderName}{Path.DirectorySeparatorChar}{_fileName}";
                        File.WriteAllBytes(_fileName, data);
                        Log.Informational(_fileName);
                    }
                }
                Log.Informational("Downloads completed");
            }
            else
            {
                Console.WriteLine($"Anime {animeName}, yeilded no results.");
            }
        }

        private static void Log_LogEvent(string message, string threadName, LogManagement.LogLevel level)
        {
            switch (level)
            {
                case LogManagement.LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogManagement.LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogManagement.LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }
            Console.WriteLine(message);
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Informational($"Terminating with error at {DateTime.Now}, with a duration of {(DateTime.Now - startedTime).TotalMinutes}");
            Log.Exception((Exception)e.ExceptionObject);
            Log.LogFileName = string.Empty;
            Environment.Exit(-1);
        }
    }
}
