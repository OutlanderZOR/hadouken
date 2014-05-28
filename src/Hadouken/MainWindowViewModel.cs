using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hadouken
{
    public class MainWindowViewModel : BindableBase
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int WM_SYSKEYDOWN = 260;
        const int WM_SYSKEYUP = 261;
        const int WM_CHAR = 258;
        const int WM_KEYDOWN = 256;
        const int WM_KEYUP = 257;

        private Task RefreshApplicationsTask;
        private Task SendKeyStrokeTask;

        private string searchString;
        public ObservableCollection<ProcessModel> applications;
        private int delay;
        private int keystrokeDelay;
        private List<string> log;

        public MainWindowViewModel()
        {

            this.Applications = new ObservableCollection<ProcessModel>();
            this.Keys = this.ValidKeys().ToArray();
            this.RefreshApplicationsTask = Task.Run(() => this.RefreshApplicationsAction());
            this.SendKeyStrokeTask = Task.Run(() => this.SendKeyStrokesAction());
            this.SendKeyStrokeSignal = new ManualResetEvent(false);
            this.Delay = 1000;
            this.KeystrokeDelay = 350;
            this.log = new List<string>();
            this.RefreshCommand = new RelayCommand(p => { this.RefreshApplications(); });
            this.StartSendingKeys = new RelayCommand(p => { this.SendKeyStrokeSignal.Set(); }, p => true);
            this.StopSendingKeys = new RelayCommand(p => { this.SendKeyStrokeSignal.Reset(); }, p => true);
        }

        public string SearchString
        {
            get { return this.searchString; }
            set
            {
                this.searchString = value;
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ProcessModel> Applications
        {
            get { return this.applications; }
            set
            {
                this.applications = value;
                this.NotifyPropertyChanged();
            }
        }
        public KeyModel[] Keys { get; set; }
        public KeyModel SelectedKey { get; set; }
        public ManualResetEvent SendKeyStrokeSignal { get; set; }

        public int Delay
        {
            get { return this.delay; }
            set
            {
                this.delay = value;
                this.NotifyPropertyChanged();
            }
        }

        public int KeystrokeDelay
        {
            get { return this.keystrokeDelay; }
            set
            {
                this.keystrokeDelay = value;
                this.NotifyPropertyChanged();
            }
        }

        public string[] Log { get; set; }

        public ICommand RefreshCommand { get; set; }
        public ICommand StartSendingKeys { get; set; }
        public ICommand StopSendingKeys { get; set; }

        private void LogMessage(string msg)
        {
            this.log.Insert(0, msg);
            if (this.log.Count > 100)
                this.log.RemoveAt(this.log.Count - 1);
            this.Log = this.log.ToArray();
            this.NotifyPropertyChanged("Log");
        }

        private ProcessModel[] SelectedApplications
        {
            get
            {
                if (this.Applications != null)
                    return this.Applications.Where(a => a.Selected).ToArray();
                else
                    return new ProcessModel[] { };
            }
        }

        private IEnumerable<KeyModel> ValidKeys()
        {
            Array array = Enum.GetValues(typeof(System.Windows.Forms.Keys));
            foreach (var item in array)
                yield return new KeyModel((int)item);
        }

        private async void RefreshApplicationsAction()
        {
            while (true)
            {
                this.RefreshApplications();
                await Task.Delay(1000);
            }
        }

        private void RefreshApplications()
        {
            Process[] process = string.IsNullOrEmpty(this.SearchString) ? Process.GetProcesses() : Process.GetProcesses().Where(a => a.ProcessName.ToLower().Contains(this.SearchString.ToLower())).ToArray();

            var unlisted = (from p in process
                            join app in this.Applications on p.Id equals app.ProcessId into g
                            from sub_app in g.DefaultIfEmpty()
                            where sub_app == null
                            select p).ToArray();

            var closed = (from app in this.Applications
                          join p in process on app.ProcessId equals p.Id into g
                          from sub_p in g.DefaultIfEmpty()
                          where sub_p == null
                          select app).ToArray();

            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in unlisted)
                    this.Applications.Add(new ProcessModel(item));

                foreach (var item in closed)
                    this.Applications.Remove(item);
            });
        }


        private async void SendKeyStrokesAction()
        {
            while (true)
            {
                if (this.SendKeyStrokeSignal.WaitOne())
                {
                    this.SendKeyStrokes();
                    await Task.Delay(this.Delay);
                }
            }
        }

        private async void SendKeyStrokes()
        {
            if (this.SelectedKey != null)
            {
                foreach (var app in this.SelectedApplications)
                {
                    PostMessage(app.WindowHandle, WM_KEYDOWN, (IntPtr)this.SelectedKey.KeyValue, IntPtr.Zero);
                    this.LogMessage(string.Format("{0} PostMessage {1}-{2} WM_KEYDOWN {3}", DateTime.Now, app.ProcessId, app.ProcessName, this.SelectedKey.DisplayValue));
                    await Task.Delay(this.KeystrokeDelay);
                }
            }
        }


    }
}
