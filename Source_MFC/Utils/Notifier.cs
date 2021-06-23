using Source_MFC.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public Notifier()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
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

    public class JobMonitor : INotifyPropertyChanged
    {
        private bool _showDismissBtn;
        private double _dismissBtnPrgrs;
        private double _DismaissBtnPrgrsMax;
        private string _ContentTxt;
        private string _demoRestartCntDwnTxt;
        private int _dismissRequested = 0;
        private bool _AlignSen = false;
        private TIMEARG _PrgrsTime = new TIMEARG();
        public JobMonitor()
        {
            var autoStartingActionCountdownStart = DateTime.Now;
            var demoRestartCountdownComplete = DateTime.Now;
            DismissComand = new AnotherCommandImplementation(_ => DismissRequested = 0);
            ShowDismissButton = true;

            new DispatcherTimer(
                TimeSpan.FromMilliseconds(5),
                DispatcherPriority.Normal,
                new EventHandler((o, e) =>
                {
                    switch (DismissRequested)
                    {
                        case 0: break;
                        case 1:
                            ShowDismissButton = false;
                            _PrgrsTime.Reset();
                            DismissButtonProgress = 0;
                            DismissRequested = 10;
                            break;
                        case 10:
                            {
                                if (false == _PrgrsTime.IsOver(500)) break;
                                _PrgrsTime.Reset();
                                ShowDismissButton = true;
                                DismissRequested = 20;
                            }
                            break;
                        case 20:
                            {
                                var isOver = _PrgrsTime.IsOver((int)(DismaissBtnPrgrsMax * 1000.0));
                                var nSec = _PrgrsTime.nCurr / 1000.0;
                                DismissButtonProgress = (nSec / DismaissBtnPrgrsMax) * 100.0;
                                DemoRestartCountdownText = $"{_ContentTxt} [{DismaissBtnPrgrsMax}/{(int)nSec} sec]";
                                if (false == isOver) break;
                                _PrgrsTime.Reset();
                                DismissRequested = 30;
                                break;
                            }
                        case 30:
                            if (false == _PrgrsTime.IsOver(500)) break;
                            ShowDismissButton = true;
                            DismissButtonProgress = 0;
                            DemoRestartCountdownText = string.Empty;
                            DismissRequested = DEF_CONST.SEQ_FINISH;
                            break;
                        default: break;
                    }

                    b_AlignSen = _AlignSen;
                    b_JobState = _JobState;
                    b_AIVState = _AIVState;
                }), Dispatcher.CurrentDispatcher);
        }

        public ICommand DismissComand { get; }

        public int DismissRequested
        {
            get { return _dismissRequested; }
            set { this.MutateVerbose(ref _dismissRequested, value, RaisePropertyChanged()); }
        }

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

        public double DismaissBtnPrgrsMax
        {
            get { return _DismaissBtnPrgrsMax; }
            set { this.MutateVerbose(ref _DismaissBtnPrgrsMax, value, RaisePropertyChanged()); }
        }

        public void StartProgress(string coment, int nMax)
        {
            DismaissBtnPrgrsMax = nMax;
            _ContentTxt = coment;
            DismissRequested = DEF_CONST.SEQ_INIT;
        }

        public void JobSet(JOB order, VEHICLESTATE vecst)
        {
            b_JobID = order.cmdID;
            b_TrayID = order.materialID;
            b_Dest = order.goal.label;
            JobState(order.state, vecst.JobState);
        }

        string _JobState = string.Empty;
        string _AIVState = string.Empty;
        public void JobState(eJOBST state, eROBOTST rbtSt)
        {
            _JobState = Ctrls.Remove_(state.ToString());
            _AIVState = Ctrls.Remove_(rbtSt.ToString());
        }


        public void SetPosGoodSen(bool bTrg)
        {
            _AlignSen = bTrg;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }

        bool senPosOk = false;
        public bool b_AlignSen
        {
            get { return senPosOk; }
            set {
                this.MutateVerbose(ref senPosOk, value, RaisePropertyChanged());
            }
        }

        string JobID = string.Empty;
        public string b_JobID
        {
            get { return JobID; }
            set { this.MutateVerbose(ref JobID, value, RaisePropertyChanged()); }
        }

        string JobType = string.Empty;
        public string b_JobType
        {
            get { return JobType; }
            set { this.MutateVerbose(ref JobType, value, RaisePropertyChanged()); }
        }

        string TrayID = string.Empty;
        public string b_TrayID
        {
            get { return TrayID; }
            set { this.MutateVerbose(ref TrayID, value, RaisePropertyChanged()); }
        }

        string Dest = string.Empty;
        public string b_Dest
        {
            get { return Dest; }
            set { this.MutateVerbose(ref Dest, value, RaisePropertyChanged()); }
        }

        string jobState = string.Empty;
        public string b_JobState
        {
            get { return jobState; }
            set {
                this.MutateVerbose(ref jobState, value, RaisePropertyChanged());
            }
        }

        string AIVState = string.Empty;
        public string b_AIVState
        {
            get { return AIVState; }
            set {
                this.MutateVerbose(ref AIVState, value, RaisePropertyChanged());
            }
        }

        string PauseState = string.Empty;
        public string b_PauseState
        {
            get { return PauseState; }
            set { this.MutateVerbose(ref PauseState, value, RaisePropertyChanged()); }
        }
    }

    
    public class SRC4MONI : Notifier
    {
        private string strEnum = string.Empty;
        public string _strEnum
        {
            set { strEnum = value; }
        }

        public eINPUT GetIn()
        {
            return strEnum.ToEnum<eINPUT>();
        }

        public eOUTPUT GetOut()
        {
            return strEnum.ToEnum<eOUTPUT>();
        }

        private bool state = false;
        public bool STATE
        {
            get => state;
            set { this.MutateVerbose(ref state, value, RaisePropertyChanged()); }
        }

        private string label = string.Empty;
        public string LABEL
        {
            get => label;
            set { this.MutateVerbose(ref label, value, RaisePropertyChanged()); }
        }        
    }

    public class SRC4ARG : Notifier
    {
        private string strSeqID= string.Empty;
        public string ID
        {
            get => strSeqID;
            set { strSeqID = value; }
        }

        public eSEQLIST GetSeqID()
        {
            return strSeqID.ToEnum<eSEQLIST>();
        }

        public eTASKLIST GetTskID()
        {
            return strSeqID.ToEnum<eTASKLIST>();
        }

        private string state = string.Empty;
        public string STATE
        {
            get => state;
            set { this.MutateVerbose(ref state, value, RaisePropertyChanged()); }
        }

        private string step = string.Empty;
        public string STEP
        {
            get => step;
            set { this.MutateVerbose(ref step, value, RaisePropertyChanged()); }
        }

        private string error = eERROR.None.ToString();
        public string ERROR
        {
            get => error;
            set { this.MutateVerbose(ref error, value, RaisePropertyChanged()); }
        }

        private bool stop = false;
        public bool STOP
        {
            get => stop;
            set { this.MutateVerbose(ref stop, value, RaisePropertyChanged()); }
        }

        private string trg = string.Empty;
        public string TRIGGER
        {
            get => trg;
            set { this.MutateVerbose(ref trg, value, RaisePropertyChanged()); }
        }

        private string rslt = string.Empty;
        public string RESULT
        {
            get => rslt;
            set { this.MutateVerbose(ref rslt, value, RaisePropertyChanged()); }
        }

        private string sen = string.Empty;
        public string SENSING
        {
            get => sen;
            set { this.MutateVerbose(ref sen, value, RaisePropertyChanged()); }
        }

        private string dly = string.Empty;
        public string DELAY
        {
            get => dly;
            set { this.MutateVerbose(ref dly, value, RaisePropertyChanged()); }
        }

    }
}

