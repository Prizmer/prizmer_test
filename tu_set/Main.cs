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
using Drivers.KaratDanfosDriver;
using Drivers.PulsarDriver;

using Drivers.LibMeter;


using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;

namespace tu_SET
{
    public partial class Main : Form
    {
        public const string FORM_TEXT = "Тестовая утилита";

        public Main()
        {
            InitializeComponent();

            this.Text = FORM_TEXT;

            if (ctlSelectDriver1.Enabled)
            {
                // если включен выбор драйвера, выключаем компонент подключения
                // т.к. сначала нужно выбрать драйвер
                ctlConnectionSettings1.Enabled = false;
            } 

            ctlConnectionSettings1.SettingsApplied += CtlConnectionSettings1_SettingsApplied;
            ctlSelectDriver1.DriverInstanceReady += CtlSelectDriver1_DriverInstanceReady;

            this.Text = FORM_TEXT + " - " + ctlSelectDriver1.PredefinedDriverName;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            bool bIsLittleEndian = BitConverter.IsLittleEndian;

        }

        private void CtlSelectDriver1_DriverInstanceReady(object sender, tu_set.DriverInstanceReadyArgs e)
        {
            bool bSuccess = !(e.MeterInterfaceInstance == null || e.MeterType == null);
            if (bSuccess)
                this.Text = FORM_TEXT + " - " + e.MeterType.Name; 
            else
                this.Text = FORM_TEXT;

            if (ctlSelectDriver1.Enabled)
            {
                ctlConnectionSettings1.ClosePortForce();
                ctlMeters1.Enabled = false;

                ctlConnectionSettings1.Enabled = bSuccess;
            }
        }

        private void CtlConnectionSettings1_SettingsApplied(object sender, PollingLibraries.LibPorts.EventArgsSettingsApplied e)
        {
            if (e.VPort == null)
            {
                ctlMeters1.Enabled = false;
                return;
            }

            // если компонент выбора драйвера включен, будем создавать объекты при помощи
            // рефлексии, иначе - задавая тип вручную.

            if (ctlSelectDriver1.Enabled)
            {
                IMeter iMeter = ctlSelectDriver1.MeterInterfaceInstance;
                ctlMeters1.Initialize(iMeter, e.VPort);
            }
            else
            {   
                // объявляем тип вручную
                SET4tmDriver meter = new SET4tmDriver();
                ctlMeters1.Initialize(meter, e.VPort);
            }

            ctlMeters1.Enabled = true;
        }

        private void ctlMeters1_Load(object sender, EventArgs e)
        {

        }


        private void btnTest_Click(object sender, EventArgs e)
        {



            // SET4tmDriver driver = (SET4tmDriver)ctlMeters1.GetInitializedDriver();

            byte[] temp = { 0x8B, 0xAE, 0x9A, 0x3B };
            double val = BitConverter.ToSingle(temp, 0) / 1000;
    


            //string tmpStrVal = val.ToString("0.0000");
            //float tmpRecordValHack = (float)Convert.ToDecimal(val);

            //List<Drivers.LibMeter.RecordPowerSlice> rps = new List<Drivers.LibMeter.RecordPowerSlice>();

            //DateTime dtNow = DateTime.Now.Date;
            //DateTime dt = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 0, 00, 00);

            //driver.ReadPowerSlice(ref rps, dt, dt, 30);

            ////byte[] temp = new byte[2] { 0x01, 0x99};

            ////// адрес последней записи
            ////int tmp = BitConverter.ToUInt16(temp, 0);



        }

        private void pictureBoxLogo_Click(object sender, EventArgs e)
        {
            Process.Start("http://prizmer.ru/");
        }
    }

    // TODO: сохранение состояний в реестр
    public static class StateSaver
    {
        static readonly string KEY_PRG_ID = typeof(Program).Assembly.GetName().Name;

        private static RegistryKey getPRGRegKey()
        {
            RegistryKey localMachine = Registry.LocalMachine;
            RegistryKey keySoftware = localMachine.OpenSubKey("SOFTWARE");
            RegistryKey target = keySoftware.OpenSubKey(KEY_PRG_ID, true);

            return target;
        }

        static void SaveData(string key, string value)
        {
            RegistryKey target = getPRGRegKey();
            target.SetValue(key, value, RegistryValueKind.MultiString);

            // нужно закрыть все открытые ранее ключи
        }

        static bool GetData(string key, ref string value)
        {

            return true;
        }

        enum DataGroups
        {
            CONNECTION_SETTINGS

        }

    }
}
