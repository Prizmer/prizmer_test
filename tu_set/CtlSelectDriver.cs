using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Reflection;
using Drivers;
using System.IO;

using Drivers.LibMeter;

namespace tu_set
{
    public partial class CtlSelectDriver : UserControl
    {
        private readonly string DIR_DRIVERS = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // + "//Drivers";
        private const string DRIVER_NAMESPACE = "Drivers";

        private IMeter _meterInterfaceInstance = null;
        public IMeter MeterInterfaceInstance
        {
            get { return _meterInterfaceInstance; }
        }

        private Type _meterType = null;
        public Type MeterType
        {
            get { return _meterType; }
        }

        private bool _driverSelected = false;
        private bool DriverSelected
        {
            get { return _driverSelected; }
            set
            {
                _driverSelected = value;
                if (!_driverSelected)
                {
                    gbDriver.Text = "Драйвер - не выбран";
                }
                else
                {
                    gbDriver.Text = $"Драйвер - {_meterType.Name}";
                }
            }
        }

        public event EventHandler<DriverInstanceReadyArgs> DriverInstanceReady;

        public CtlSelectDriver()
        {
            InitializeComponent();
        }

        private void loadDriverAssemblies()
        {
            if (Directory.Exists(DIR_DRIVERS))
            {
                List<Assembly> driverAssemblies = new List<Assembly>();
                foreach (string dll in Directory.GetFiles(DIR_DRIVERS, "*.dll"))
                    driverAssemblies.Add(Assembly.LoadFile(dll));
            }
        }


        private List<Type> getDriverTypes()
        {
            List<Type> res = new List<Type>();
            try
            {
                Assembly[] curDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                var q = curDomainAssemblies
                           .SelectMany(t => t.GetTypes())
                           .Where(t => t.IsClass && t.Namespace != null && t.Namespace.Contains(DRIVER_NAMESPACE) && t.Name.ToLower().Contains("driver"));

                q.ToList().ForEach(t => res.Add(t));
            }
            catch (Exception ex)
            {
                // введен для того, чтобы элемент отрисовывался в конструкторе
                // без трай кэтч, конструктор рушится
            }

            return res;
        }

        public List<string> GetDriverNames()
        {
            List<string> res = new List<string>();

            List<Type> driverTypesList = getDriverTypes();
            driverTypesList.ForEach((t) => res.Add(t.Name));

            return res;
        }

        private void CtlSelectDriver_Load(object sender, EventArgs e)
        {
            loadDriverAssemblies();

            List<string> driverNames = GetDriverNames();
            cbDriverName.Items.AddRange(driverNames.ToArray());

            selectComboboxItemByNamePart(PredefinedDriverName);

            if (cbDriverName.SelectedIndex > -1)
                applyDriver();
        }

        private bool selectComboboxItemByNamePart(string namePart)
        {
            if (cbDriverName.Items.Count == 0) return false;

            if (namePart == "")
            {
                cbDriverName.SelectedIndex = 0;
                return false;
            }

            int idx = -1;
            foreach (object o in cbDriverName.Items)
            {
                idx++;
                string itemText = o.ToString().ToLower();
                if (itemText.Contains(namePart.ToLower()))
                {
                    cbDriverName.SelectedIndex = idx;
                    break;
                }
            }

            return idx > -1;
        }


        private void applyDriver()
        {
            int idxDriver = cbDriverName.SelectedIndex;
            if (idxDriver == -1)
            {
                DriverSelected = false;
                DriverInstanceReady?.Invoke(this, new DriverInstanceReadyArgs(null, null));
                return;
            }

            List<Type> driverTypesList = getDriverTypes();
            if (idxDriver >= driverTypesList.Count)
            {
                DriverSelected = false;
                DriverInstanceReady?.Invoke(this, new DriverInstanceReadyArgs(null, null));
                return;
            }

            _meterType = driverTypesList[idxDriver];
            _meterInterfaceInstance = (IMeter)Activator.CreateInstance(MeterType);
            DriverSelected = true;

            DriverInstanceReady?.Invoke(this, new DriverInstanceReadyArgs(MeterInterfaceInstance, MeterType));
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            applyDriver();
        }

        #region Свойства компонента

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Предустановленное имя драйвера"), Category("Custom")]
        private string _predefinedDriverName = "";
        public string PredefinedDriverName
        {
            get
            {
                return _predefinedDriverName;
            }
            set
            {
                _predefinedDriverName = value;
            }
        }

        #endregion
    }

    public class DriverInstanceReadyArgs : EventArgs
    {
        public readonly IMeter MeterInterfaceInstance = null;
        public readonly Type MeterType = null;

        public DriverInstanceReadyArgs(IMeter iMeter, Type meterType)
        {
            MeterInterfaceInstance = iMeter;
            MeterType = meterType;
        }
    }
}
