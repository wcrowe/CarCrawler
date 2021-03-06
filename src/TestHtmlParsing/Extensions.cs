﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestHtmlParsing
{
    internal static class Extensions
    {
        public static IEnumerable<AngleSharp.Dom.IElement> GetDeepControlsByType<T>(this AngleSharp.Dom.IElement control)
        {
            foreach (var c in control.Children)
            {
                if (c is T)
                {
                    yield return c;
                }

                if (c.Children.Length > 0)
                {
                    foreach (var ctrl in c.GetDeepControlsByType<T>())
                    {
                        yield return ctrl;
                    }
                }
            }
        }

        public static string AppendLine(this string text)
        {
            return text + System.Environment.NewLine;
        }

        public static string NullIfEmpty(this string text)
        {
            var s = text?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }
    }

}