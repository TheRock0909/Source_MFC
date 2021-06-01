using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Source_MFC.Utils
{
    public static class NotifyPropertyChangedExtension
    {
        public static bool MutateVerbose<TField>(this INotifyPropertyChanged instance, ref TField field, TField newValue, Action<PropertyChangedEventArgs> raise, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TField>.Default.Equals(field, newValue)) return false;
            field = newValue;
            raise?.Invoke(new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

    public class Notifier : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ITextBoxAppend
    {
        void Delete();
        void Delete(int startIndex, int length);

        void Append(string value);
        void Append(string value, int index);

        string GetCurrVal();

        event EventHandler<string> Evt_BuffAppendHdr;
        event EventHandler<string> Evt_BuffDeleteHdr;
    }

    class IClassTextBoxAppend : ITextBoxAppend
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        public void Delete()
        {
            _buffer.Clear();
            System.Windows.Application.Current.Dispatcher?.Invoke(new Action(() =>
            {
                Evt_BuffDeleteHdr?.Invoke(this, null);
            }));
        }

        public void Delete(int startIndex, int length)
        {
            _buffer.Remove(startIndex, length);
        }

        public void Append(string value)
        {
            _buffer.Append(value);
            App.Current.Dispatcher.Invoke((Action)(() =>
            {
                Evt_BuffAppendHdr?.Invoke(this, value);
            }));            
        }

        public void Append(string value, int index)
        {
            if (index == _buffer.Length)
            {
                _buffer.Append(value);
            }
            else
            {
                _buffer.Insert(index, value);
            }
        }

        public string GetCurrVal()
        {
            return _buffer.ToString();
        }

        public event EventHandler<string> Evt_BuffAppendHdr;
        public event EventHandler<string> Evt_BuffDeleteHdr;
    }

    public class BtnViewMdl : INotifyPropertyChanged
    {
        private bool _showDismissBtn;
        private double _dismissBtnPrgrs;
        private string _demoRestartCntDwnTxt;
        public BtnViewMdl()
        {
            var autoStartingActionCountdownStart = DateTime.Now;
            var demoRestartCountdownComplete = DateTime.Now;
            var dismissRequested = false;

            DismissComand = new AnotherCommandImplementation(_ => dismissRequested = true);
            ShowDismissButton = true;

            new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Normal,
                new EventHandler((o, e) =>
                {
                    if (dismissRequested)
                    {
                        ShowDismissButton = false;
                        dismissRequested = false;
                        demoRestartCountdownComplete = DateTime.Now.AddSeconds(3);
                        DismissButtonProgress = 0;
                    }

                    if (ShowDismissButton)
                    {
                        var totalDuration = autoStartingActionCountdownStart.AddSeconds(5).Ticks - autoStartingActionCountdownStart.Ticks;
                        var currentDuration = DateTime.Now.Ticks - autoStartingActionCountdownStart.Ticks;
                        var autoCountdownPercentComplete = 100.0 / totalDuration * currentDuration;
                        DismissButtonProgress = autoCountdownPercentComplete;

                        if (DismissButtonProgress >= 100)
                        {
                            demoRestartCountdownComplete = DateTime.Now.AddSeconds(3);
                            ShowDismissButton = false;
                            UpdateDemoRestartCountdownText(demoRestartCountdownComplete, out _);
                        }
                    }
                    else
                    {
                        UpdateDemoRestartCountdownText(demoRestartCountdownComplete, out bool isComplete);
                        if (isComplete)
                        {
                            autoStartingActionCountdownStart = DateTime.Now;
                            ShowDismissButton = true;
                        }
                    }

                }), Dispatcher.CurrentDispatcher);
        }

        public ICommand DismissComand { get; }

        public bool ShowDismissButton
        {
            get { return _showDismissBtn; }
            set { this.MutateVerbose(ref _showDismissBtn, value, RaisePropertyChanged()); }
        }

        public double DismissButtonProgress
        {
            get { return _dismissBtnPrgrs; }
            set { this.MutateVerbose(ref _dismissBtnPrgrs, value, RaisePropertyChanged()); }
        }

        public string DemoRestartCountdownText
        {
            get { return _demoRestartCntDwnTxt; }
            private set { this.MutateVerbose(ref _demoRestartCntDwnTxt, value, RaisePropertyChanged()); }
        }

        private void UpdateDemoRestartCountdownText(DateTime endTime, out bool isComplete)
        {
            var span = endTime - DateTime.Now;
            var seconds = Math.Round(span.TotalSeconds < 0 ? 0 : span.TotalSeconds);
            //DemoRestartCountdownText = "Demo in " + seconds;
            isComplete = seconds == 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }

}

