using Source_MFC.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Source_MFC.Global
{
    public class Ctrls
    {
        public static string FormatBytes(long bytes)
        {
            const int scale = 1024;
            string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);

            foreach (string order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);

                max /= scale;
            }
            return "0 Bytes";
        }   
        
        public static string Remove_(string enumstr)
        {
            return enumstr.Replace("_", " ").ToUpper();
        }

        public static string Remove_line(string str)
        {
            var regex = new Regex(@"\r\n?|\n|\t", RegexOptions.Compiled);
            string rtn = regex.Replace(str, String.Empty);
            return rtn;
        }
    }

    public class Noti
    {
        public bool bTrg { get; set; } = false;
        public string msg { get; set; } = string.Empty;
        public int nTemp { get; set; } = 0;
        public double dTemp { get; set; } = 0;
    }


    public class MvvmTxtBox
    {
        public static readonly DependencyProperty BufferProperty 
            = DependencyProperty.RegisterAttached("Buffer", typeof(ITextBoxAppend), typeof(MvvmTxtBox), new UIPropertyMetadata(null, PropertyChangedCallback) );
        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs depPropChangedEvArgs)
        {
            // todo: unrelease old buffer.
            var textBox = (TextBox)dependencyObject;
            var textBuffer = (ITextBoxAppend)depPropChangedEvArgs.NewValue;

            var detectChanges = true;

            textBox.Text = textBuffer.GetCurrVal();
            textBuffer.Evt_BuffAppendHdr += (sender, appendedText) =>
            {
                detectChanges = false;
                textBox.AppendText(appendedText);
                textBox.ScrollToEnd();
                detectChanges = true;
            };

            textBuffer.Evt_BuffDeleteHdr += (sender, args) =>
            {
                detectChanges = false;
                textBox.Clear();
                detectChanges = true;
            };

            // todo: unrelease event handlers.
            textBox.TextChanged += (sender, args) =>
            {
                if (!detectChanges) return;

                foreach (var change in args.Changes)
                {
                    if (change.AddedLength > 0)
                    {
                        var addedContent = textBox.Text.Substring(
                            change.Offset, change.AddedLength);

                        textBuffer.Append(addedContent, change.Offset);                        
                    }
                    else
                    {
                        textBuffer.Delete(change.Offset, change.RemovedLength);
                    }
                }
                Debug.WriteLine(textBuffer.GetCurrVal());
            };
        }

        public static void SetBuffer(UIElement element, bool value)
        {
            element.SetValue(BufferProperty, value);
        }
        public static ITextBoxAppend GetBuffer(UIElement element)
        {
            return (ITextBoxAppend)element.GetValue(BufferProperty);
        }
    }    
}
