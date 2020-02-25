using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogManagement
{
    public static class Log
    {
        private static string logFileName;
        private static FileStream logFileStream;
        private static bool closeLogFile = false;

        public static string DefaultLogFileName {
            get {
                return $"{Assembly.GetExecutingAssembly().FullName.Split(',').First()} [{DateTime.Now.ToLongDateString()}, {DateTime.Now.ToLongTimeString().Replace(':', '.')}].log";
            }
        }

        public static string LogFileName {
            get { return logFileName; }
            set {
                logFileName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    logFileStream = File.Open(value, FileMode.Append, FileAccess.Write, FileShare.Read);
                    logFileStream.Seek(0, SeekOrigin.End);
                    LogEvent += LogToFile;
                }
                else
                {
                    if (logFileStream != null)
                    {
                        string _composedMessage = $"[{DateTime.Now.ToShortDateString()},{DateTime.Now.ToLongTimeString()}] Log Stream Closed";
                        new StreamWriter(logFileStream).WriteLine("---------------------------------------------------------------------");
                        logFileStream.Flush();
                        LogEvent -= LogToFile;
                        closeLogFile = true;
                    }
                }
            }
        }

        private static void LogToFile(string message, string threadName, LogLevel level)
        {
            if (logFileStream.CanWrite)
            {
                new StreamWriter(logFileStream) { AutoFlush = true }.WriteLine($"{message}");
                logFileStream.Flush();
            }
            if (closeLogFile)
            {
                logFileStream.Close();
                closeLogFile = false;
            }
        }

        public static void Informational(string message)
        {
            Write(message, LogLevel.Information);
        }

        public static void Warning(string message)
        {
            Write(message, LogLevel.Warning);
        }

        public static void Error(string message)
        {
            Write(message, LogLevel.Error);
        }

        public static void Exception(Exception ex)
        {
            Write("Message: " + ex.Message + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString(), LogLevel.Error);
        }

        public static void Write(string message, LogLevel level)
        {
            string _threadName = Thread.CurrentThread.Name;
            string _composedMessage = $"[{DateTime.Now.ToShortDateString()},{DateTime.Now.ToLongTimeString()}] {message}";
            if (LogEvent != null)
            {
                foreach (LogEventDelegate _delegate in LogEvent.GetInvocationList())
                {
                    _delegate.Invoke(_composedMessage, _threadName, level);
                }
            }
        }

        public delegate void LogEventDelegate(string message, string threadName, LogLevel level);
        public static event LogEventDelegate LogEvent;
    }

    public enum LogLevel : int
    {
        Information = 1,
        Warning = 2,
        Error = 3
    }
}
