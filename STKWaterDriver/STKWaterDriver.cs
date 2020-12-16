using Drivers.LibMeter;
using PollingLibraries.LibPorts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CRCCalc;

namespace Drivers.STKWaterDriver
{
    public class STKWaterDriver : CMeter, IMeter
    {

        private LibCRC libCrc = new LibCRC();
        private const byte REQ_CMD_1 = 0xAA;
        
        public void Init(uint address, string pass, VirtualPort data_vport)
        {
            this.m_address = address;
            this.m_vport = data_vport;

            libCrc = new LibCRC(LibCRC.Algorithms.crc8Dallas);
        }

        public bool OpenLinkCanal()
        {
            string swVersion = "";
            return true;//readSoftwareVersion(ref swVersion);
        }

        private bool doReverse()
        {
            return !BitConverter.IsLittleEndian;
        }

        private List<byte> makeRequest(uint address, List<byte> data)
        {
            List<byte> request = new List<byte>();

            request.Add(0x02);

            byte[] addressBytes = BitConverter.GetBytes((ushort)address);
            if (doReverse()) Array.Reverse(addressBytes);
            request.AddRange(addressBytes);

            byte[] lngBytes = BitConverter.GetBytes((ushort)data.Count);
            if (doReverse()) Array.Reverse(lngBytes);
            request.AddRange(lngBytes);

            request.AddRange(data);

            byte[] crcArr = { };
            bool crcResult = libCrc.GetCRCFromByteArr(request.ToArray(), ref crcArr);

            if (crcResult)
            {
                request.Add(crcArr[0]);
            } 
            else
            {
                WriteToLog("makeRequest crc calculation error");
                return null;
            }

            return request;
        }
        public int findPackageSign(Queue<byte> queue)
        {
            return isCRCCorrect(queue.ToArray()) ? 1 : 0;
        }

        private bool isCRCCorrect(byte[] dataWithCRC)
        {
            byte[] crcArr = { };
            bool crcResult = libCrc.GetCRCFromByteArr(dataWithCRC, ref crcArr);

            if (!crcResult || crcArr[0] != 0)
                return false;
            else
                return true;
        }
 
        private bool readSoftwareVersion(ref string version)
        {
            version = String.Empty;

            List<byte> data = new List<byte> { REQ_CMD_1, 0x0A };
            List<byte> cmd = makeRequest(m_address, data);
            byte[] receivedData = null;


            int readBytes = m_vport.WriteReadData(findPackageSign, cmd.ToArray(), ref receivedData, cmd.Count, -1);
            if (readBytes == 0)
            {
                WriteToLog("ReadSoftwareVersion WriteReadData returns 0 (readBytes=0)");
                return false;
            }

            if (!isCRCCorrect(receivedData))
            {
                WriteToLog("ReadSoftwareVersion isCRCCorrect returns false");
                return false;
            }

            List<byte> receivedDataList = new List<byte>(receivedData);
            try
            {
                List<byte> swVersionBytes = receivedDataList.GetRange(8, 15);
                version = ASCIIEncoding.ASCII.GetString(swVersionBytes.ToArray());
                return version.Length > 0;
                
            } catch (Exception e)
            {
                WriteToLog("ReadSoftwareVersion: " + e.ToString());
                return false;
            }
        }



        private uint getCounterValue(byte[] countersData, int counterIdx = 1)
        {
            List<byte> countersDataList = new List<byte>(countersData);
            int startByteIdx = counterIdx * 4;

            try
            {
                List<byte> valueBytes = countersDataList.GetRange(startByteIdx, 4);
                if (doReverse()) valueBytes.Reverse();

                uint val = BitConverter.ToUInt32(valueBytes.ToArray(), 0);
                return val;

            } catch (Exception e)
            {
                WriteToLog(String.Format("getCounterValue: counter idx {0}, error: {1}", startByteIdx, e.ToString()));
                return 0;
            }
        }

        public bool ReadCurrentValues(ushort param, ushort tarif, ref float recordValue)
        {
            recordValue = -1;

            List<byte> data = new List<byte> { REQ_CMD_1, 0x0E };
            List<byte> cmd = makeRequest(m_address, data);
            byte[] receivedData = null;


            int readBytes = m_vport.WriteReadData(findPackageSign, cmd.ToArray(), ref receivedData, cmd.Count, -1);
            if (readBytes == 0)
            {
                WriteToLog("ReadCurrentValues WriteReadData returns 0 (readBytes=0)");
                return false;
            }

            if (!isCRCCorrect(receivedData))
            {
                WriteToLog("ReadCurrentValues isCRCCorrect returns false");
                return false;
            }

            //val = 65500
            //receivedDataTest = new byte[] { 0x82, 0xfe, 0xff, 0x12, 0x00, 0xaa, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdc, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2d };

            List<byte> receivedDataList = new List<byte>(receivedData);
            try
            {
                List<byte> counterDataBytes = receivedDataList.GetRange(7, 16);
                // у приборов данной версии 4 регистратора, работает только 2й, т.е. с индексом 1
                uint value = getCounterValue(counterDataBytes.ToArray(), 1);
                recordValue = (float)value;
                return true;

            }
            catch (Exception e)
            {
                WriteToLog("ReadCurrentValues: " + e.ToString());
                return false;
            }
        }

        private bool readLastRecordIdx(ref ushort lastRecordIdx)
        {
            lastRecordIdx = 0;

            List<byte> data = new List<byte> { REQ_CMD_1, 0x20, 0x01 };
            List<byte> cmd = makeRequest(m_address, data);
            byte[] receivedData = null;


            int readBytes = m_vport.WriteReadData(findPackageSign, cmd.ToArray(), ref receivedData, cmd.Count, -1);
            if (readBytes == 0)
            {
                WriteToLog("readLastRecordIdx WriteReadData returns 0 (readBytes=0)");
                return false;
            }

            if (!isCRCCorrect(receivedData))
            {
                WriteToLog("readLastRecordIdx isCRCCorrect returns false");
                return false;
            }

            List<byte> receivedDataList = new List<byte>(receivedData);
            try
            {
                List<byte> recordIdxDataBytes = receivedDataList.GetRange(7, 2);
                if (doReverse()) recordIdxDataBytes.Reverse();


                lastRecordIdx = BitConverter.ToUInt16(recordIdxDataBytes.ToArray(), 0);
                return true;

            }
            catch (Exception e)
            {
                WriteToLog("ReadCurrentValues: " + e.ToString());
                return false;
            }
        }

        public bool ReadDailyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            return ReadCurrentValues(param, tarif, ref recordValue);
        }

        public bool ReadMonthlyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            ushort lastRecordIdx = 0;
            if (!readLastRecordIdx(ref lastRecordIdx))
            {
                WriteToLog("Ошибка чтения readLastRecordIdx");
                return false;
            }

            int diffInMonths = ((DateTime.Now.Date.Year - dt.Date.Year) * 12) + DateTime.Now.Date.Month - dt.Month;

            if (diffInMonths > lastRecordIdx)
            {
                WriteToLog(String.Format("ReadDailyValues индекс последней записи счетчика readLastRecordIdx={0}, а требуется запись за {1} месяцев назад",
                    lastRecordIdx, diffInMonths));
                return false;
            }

            ushort targetRecordIdx = (ushort)(lastRecordIdx - diffInMonths);

            List<byte> data = new List<byte> { REQ_CMD_1, 0x20, 0x02 };

            byte[] idx = BitConverter.GetBytes(targetRecordIdx);
            data.AddRange(idx);

            // значения параметров ниже не важны при чтении записи по индексу
            byte id = 0x00;
            data.Add(id);

            byte[] date = new byte[] { 0x14, 0x0c, 0x01 };
            data.AddRange(date);

            byte[] vals = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            data.AddRange(vals);

            byte status = 0;
            data.Add(status);

            byte valsCRC = 0;
            data.Add(valsCRC);

            List<byte> cmd = makeRequest(m_address, data);

            byte[] receivedData = null;

            int readBytes = m_vport.WriteReadData(findPackageSign, cmd.ToArray(), ref receivedData, cmd.Count, -1);
            if (readBytes == 0)
            {
                WriteToLog("ReadDailyValues WriteReadData returns 0 (readBytes=0)");
                return false;
            }

            if (!isCRCCorrect(receivedData))
            {
                WriteToLog("ReadDailyValues isCRCCorrect returns false");
                return false;
            }

            //val = 0x33 (51d)
            //receivedDataTest = new byte[] { 0x02, 0xfe, 0xff, 0x1b, 0x00, 0xaa, 0x20, 0x02, 0x10, 0x00, 0x10, 0x14, 0x0c, 0x01, 0x00, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x84, 0xef };

            List<byte> receivedDataList = new List<byte>(receivedData);
            try
            {
                // кроме значений счетчиков ничего читать не будем
                List<byte> counterDataBytes = receivedDataList.GetRange(13, 16);
                // у приборов данной версии 4 регистратора, работает только 2й, т.е. с индексом 1
                uint value = getCounterValue(counterDataBytes.ToArray(), 1);
                recordValue = (float)value;
                return true;

            }
            catch (Exception e)
            {
                WriteToLog("ReadCurrentValues: " + e.ToString());
                return false;
            }
        }


        public bool ReadSerialNumber(ref string serial_number)
        {
            // TODO: читать серийник
            serial_number = String.Empty;
            return readSoftwareVersion(ref serial_number);
        }


        #region Не используемые

        public bool ReadDailyValues(uint recordId, ushort param, ushort tarif, ref float recordValue)
        {
            return false;
        }

        public bool ReadPowerSlice(DateTime dt_begin, DateTime dt_end, ref List<RecordPowerSlice> listRPS, byte period)
        {
            return false;
        }

        public bool ReadPowerSlice(ref List<SliceDescriptor> sliceUniversalList, DateTime dt_end, SlicePeriod period)
        {
            return false;
        }

        public bool ReadSliceArrInitializationDate(ref DateTime lastInitDt)
        {
            return false;
        }

        public bool SyncTime(DateTime dt)
        {
            return false;
        }

        public List<byte> GetTypesForCategory(CommonCategory common_category)
        {
            return null;
        }

        #endregion
    }
}
