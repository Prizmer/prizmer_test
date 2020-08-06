using Drivers.LibMeter;
using PollingLibraries.LibPorts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drivers.KaratDanfosDriver
{
    public class IndivDriver : CMeter, IMeter
    {

        public void Init(uint address, string pass, VirtualPort data_vport)
        {
            throw new NotImplementedException();
        }


        public bool OpenLinkCanal()
        {
            throw new NotImplementedException();
        }

        public bool ReadCurrentValues(ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }




        public List<byte> GetTypesForCategory(CommonCategory common_category)
        {
            throw new NotImplementedException();
        }

        public bool ReadDailyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadDailyValues(uint recordId, ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadMonthlyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadPowerSlice(DateTime dt_begin, DateTime dt_end, ref List<RecordPowerSlice> listRPS, byte period)
        {
            throw new NotImplementedException();
        }

        public bool ReadPowerSlice(ref List<SliceDescriptor> sliceUniversalList, DateTime dt_end, SlicePeriod period)
        {
            throw new NotImplementedException();
        }

        public bool ReadSerialNumber(ref string serial_number)
        {
            throw new NotImplementedException();
        }

        public bool ReadSliceArrInitializationDate(ref DateTime lastInitDt)
        {
            throw new NotImplementedException();
        }

        public bool SyncTime(DateTime dt)
        {
            throw new NotImplementedException();
        }
    }
}
