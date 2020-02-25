using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace HorribleSubs
{
    public static class Extensions
    {

        public static string XPath(this IWebElement element)
        {
            var parent = element.FindElement(By.XPath(".."));
            if (parent == null)
            {
                return string.Empty;
            }
            else
            {
                var _childIndex = parent.FindElements(By.XPath("*")).IndexOf(element) + 1;
                var _parentXPath = parent.XPath();
                return _parentXPath != string.Empty ? $"{_parentXPath}/{element.TagName}[{_childIndex}]" : $"{element.TagName}[1]";
            }
        }

        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            var list = new List<T>();
            while (e.MoveNext())
            {
                list.Add(e.Current);
            }
            return list;
        }
        public static List<T> Reversed<T>(this List<T> e)
        {
            e.Reverse();
            return e;
        }
    }
}
