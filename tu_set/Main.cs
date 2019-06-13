using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Drivers;

namespace tu_SET
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent(); 
        }

        private void Main_Load(object sender, EventArgs e)
        {
            ctlConnectionSettings1.SettingsApplied += CtlConnectionSettings1_SettingsApplied;

        }

        private void CtlConnectionSettings1_SettingsApplied(object sender, PollingLibraries.LibPorts.EventArgsSettingsApplied e)
        {
            if (e.VPort != null)
            {
                ctlMeters1.Enabled = true;
                Drivers.SET4tmDriver meter = new SET4tmDriver();
                ctlMeters1.InitializeMeter(meter, e.VPort);
            } else
            {
                ctlMeters1.Enabled = false;
            }
        }
    }
}
