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

        private void ctlMeters1_Load(object sender, EventArgs e)
        {

        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //SET4tmDriver driver = new SET4tmDriver();
            //List<Drivers.LibMeter.RecordPowerSlice> rps = new List<Drivers.LibMeter.RecordPowerSlice>();

            //DateTime dtNow = DateTime.Now.Date;
            //DateTime dt = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 20, 30, 00);

            //driver.ReadPowerSlice(ref rps, dt, 30);

            byte[] temp = new byte[2] { 0x01, 0x99};

            // адрес последней записи
            int tmp = BitConverter.ToUInt16(temp, 0);


        }
    }
}
