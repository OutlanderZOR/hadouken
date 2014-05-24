using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hadouken
{
    public class ProcessModel : BindableBase
    {
        private bool selected;

        public ProcessModel(Process process)
        {
            this.Selected = false;
            this.ProcessId = process.Id;
            this.ProcessName = process.ProcessName;
            try
            {
                this.StartTime = process.StartTime;
            }
            catch { }

            this.WindowHandle = process.MainWindowHandle;
        }

        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                this.NotifyPropertyChanged();
            }
        }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public DateTime StartTime { get; set; }
        public IntPtr WindowHandle { get; set; }
    }
}
