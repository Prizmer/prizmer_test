﻿using Drivers.LibMBus;
using Drivers.LibMeter;
using PollingLibraries.LibPorts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Drivers.STKHeatDriver
{
    public class STKHeatDriver : MBusDriver, IMeter
    {

        bool isMeterSelected = false;

        public void Init(uint address, string pass, VirtualPort vp)
        {
            this.m_address = address;
            this.m_vport = vp;

            isMeterSelected = false;
            cachedAnsewerBytes.Clear();
        }


        bool bLogOutBytes = true;

        public STKHeatDriver() { }

        #region Протокол MBUS

        // жестко сопоставляет тип записи в ответе RSP_UD2 с ее порядковым
        // номером в этом ответе
        public enum Params
        {
            ENERGY = 0,
            ENERGY2 = 1,
            VOLUME = 2,
            TEMP_INP = 5,
            TEMP_OUTP = 6,

            VOLUME_FLOW = 4,
            POWER = 3
        }

        private bool getRecordValueByParam(Params param, List<Record> records, out float value)
        {
            if (records == null && records.Count == 0)
            {
                WriteToLog("getRecordValueByParam: список записей пуст");
                value = 0f;
                return false;
            }

            if ((int)param >= records.Count)
            {
                WriteToLog("getRecordValueByParam: параметра не существует в списке записей: " + param.ToString());
                value = 0f;
                return false;
            }

            Record record = records[(int)param];
            byte[] data = record.dataBytes.ToArray();
            Array.Reverse(data);
            string hex_str = BitConverter.ToString(data).Replace("-", string.Empty);

            //коэффициент, на который умножается число, полученное со счетчика
            float COEFFICIENT = 1;

            RecordDataType rdt = RecordDataType.NO_DATA;
            int dataBytesCnt = GetLengthAndTypeFromDIF(record.DIF, out rdt);

            switch (param)
            {
                case Params.ENERGY:
                case Params.ENERGY2:
                case Params.POWER:
                    {
                        COEFFICIENT = (float)Math.Pow(10, 1);
                        break;
                    }
                case Params.VOLUME:
                    {
                        COEFFICIENT = (float)Math.Pow(10, -2);
                        break;
                    }
                case Params.VOLUME_FLOW:
                    {
                        COEFFICIENT = (float)Math.Pow(10, -3);
                        break;
                    }
                case Params.TEMP_INP:
                case Params.TEMP_OUTP:
                    {
                        COEFFICIENT = (float)Math.Pow(10, -2);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            int tmpVal = -1;
            value = tmpVal;


            // все данные, кроме метки времени - BCD, поэтому преобразовываем все и применяем коэффициенты

            try
            {
                tmpVal = BCDToNumber(data);
            }
            catch (Exception ex)
            {
                string mgs = String.Format("Ошибка преобразования байт параметра {0}, исходная строка: {1}, преобразуемое кол-во байт: {2}, исключение {3}",
                     param.ToString(), hex_str, dataBytesCnt, ex.Message);
                WriteToLog(mgs);

                return false;
            }

            try
            {
                value = (float)Math.Round(tmpVal * COEFFICIENT, 2, MidpointRounding.AwayFromZero);
            } catch (Exception ex)
            {
                string mgs = String.Format("Ошибка при домножении на коэффициент {0} значения {1}, исходная строка: {2}, исключение {3}",
                    COEFFICIENT, tmpVal, hex_str, ex.Message);
                WriteToLog(mgs);

                return false;
            }


            return true;
        }

        public bool SendREQ_UD2(out List<byte> recordsBytesList)
        {
            recordsBytesList = new List<byte>();

            //byte cmd = 0x7b;
            //if (!FCV)
            //    cmd = 0x4b;
            byte cmd = 0x5B;
            byte addr = 0xFD;

            byte CS = (byte)(cmd + addr);

            byte[] cmdArr = { 0x10, cmd, addr, CS, 0x16 };
            byte[] inp = new byte[256];

            try
            {
                // TODO: вернуть
                m_vport.WriteReadData(findPackageSign, cmdArr, ref inp, cmdArr.Length, -1);

                // тестовые данные
                // inp = new byte[] { 0x68, 0xC2, 0xC2, 0x68, 0x08, 0x57, 0x72, 0x87, 0x45, 0x70, 0x73, 0xD3, 0x10, 0x02, 0x0C, 0x20, 0x00, 0x00, 0x00, 0x04, 0xFB, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x40, 0xFB, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x04, 0x14, 0x09, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x40, 0x14, 0x00, 0x00, 0x00, 0x00, 0x04, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x04, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x02, 0x5A, 0x9B, 0x00, 0x02, 0x5E, 0x9A, 0x00, 0x02, 0x61, 0x05, 0x00, 0x02, 0x66, 0xC5, 0x00, 0x04, 0x6D, 0x30, 0x30, 0x75, 0x2A, 0x04, 0x22, 0xF3, 0x47, 0x00, 0x00, 0x04, 0x26, 0x46, 0x02, 0x00, 0x00, 0x0F, 0x00, 0x00, 0xF8, 0x05, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x7F, 0xF1, 0x17, 0x7F, 0x17, 0x7F, 0xF2, 0x17, 0xF1, 0x17, 0x7F, 0xF1, 0x7F, 0xF1, 0x17, 0x7F, 0x17, 0x7F, 0xF1, 0x17, 0xF1, 0x17, 0x7F, 0xF1, 0x7F, 0xF2, 0x07, 0x7F, 0x17, 0x7F, 0xF1, 0x17, 0xF1, 0x17, 0x7F, 0xF1, 0x7F, 0xF1, 0x17, 0x7F, 0x27, 0x7F, 0xF1, 0x07, 0xF3, 0x37, 0x7F, 0xF3, 0x7E, 0xF1, 0x37, 0x7F, 0x57, 0x7E, 0xE9, 0xE7, 0xF3, 0x87, 0x7E, 0xE4, 0x81, 0x14, 0x48, 0x80, 0x27, 0x80, 0x19, 0xE8, 0x90, 0x17, 0x7B, 0xDC, 0x7E, 0xA7, 0xA7, 0x78, 0x38, 0x88, 0x39, 0x78, 0x53, 0xC8, 0x89, 0xAA, 0x70, 0x62, 0x07, 0x7E, 0x77, 0x70, 0xDC, 0x06, 0x7C, 0x16 };



                string answ_str = "";
                if (bLogOutBytes)
                {
                    answ_str = String.Format("SendREQ_UD2 (row): [{0}];", BitConverter.ToString(inp).Replace("-", " "));
                    WriteToLog(answ_str);
                }

                if (inp.Length < 6)
                {
                    WriteToLog("SendREQ_UD2: Длина корректного ответа не может быть меньше 5 байт: " + answ_str);
                    return false;
                }

                int firstAnswerByteIndex = -1;
                int byteCIndex = -1;
                //определим индекс первого байта С
                for (int i = 0; i < inp.Length; i++)
                {
                    int j = i + 3;
                    if (inp[i] == 0x68 && j < inp.Length && inp[j] == 0x68)
                    {
                        firstAnswerByteIndex = i;
                        byteCIndex = ++j;
                    }
                }

                if (firstAnswerByteIndex == -1)
                {
                    WriteToLog("SendREQ_UD2: не определено начало ответа 0x68, firstAnswerByteIndex: " + firstAnswerByteIndex.ToString());
                    return false;
                }

                //определим длину данных ответа
                byte dataLength = inp[firstAnswerByteIndex + 1];
                if (dataLength != inp[firstAnswerByteIndex + 2])
                {
                    WriteToLog("SendREQ_UD2: не определена длина данных L, dataLength");
                    return false;
                }


                byte C = inp[byteCIndex];
                byte A = inp[byteCIndex + 1]; //адрес прибора 
                byte CI = inp[byteCIndex + 2]; //тип ответа, если 72h то с переменной длиной

                if (CI != 0x72)
                {
                    WriteToLog("SendREQ_UD2: счетчик должен ответить сообщением с переменной длиной, CI = 0x72");
                    return false;
                }

                int firstFixedDataHeaderIndex = byteCIndex + 3;
                byte[] factoryNumberBytes = new byte[4];
                Array.Copy(inp, firstFixedDataHeaderIndex, factoryNumberBytes, 0, factoryNumberBytes.Length);
                Array.Reverse(factoryNumberBytes);
                //серийный номер полученный из заголовка может быть изменен, достовернее серийник, полученный из блока записей
                string factoryNumber = BitConverter.ToString(factoryNumberBytes);

                //12 байт - размер заголовка, индекс первого байта первой записи
                int firstRecordByteIndex = firstFixedDataHeaderIndex + 12;

                //байт окончания сообщения
                int lastByteIndex = byteCIndex + dataLength + 1;
                if (inp[lastByteIndex] != 0x16)
                {
                    WriteToLog("SendREQ_UD2: не найден байт окончания сообщения 0х16");
                    return false;
                }

                int byteCSIndex = byteCIndex + dataLength;
                byte byteCSVal = inp[byteCSIndex];
                byte byteCSEvaluated = 0;

                //расчитаем контрольную сумму по данным ответа и сравним ее с присланной счетчиком
                for (int i = byteCIndex; i < byteCSIndex; i++)
                    byteCSEvaluated += inp[i];

                if (byteCSEvaluated != byteCSVal)
                {
                    string msg = String.Format("SendREQ_UD2: рассчитанная контрольная сумма ({0}) не соответствует сумме, рассчитанной счетчиком ({1}) - данные некорректны. ",
                        Convert.ToString(byteCSEvaluated, 16), Convert.ToString(byteCSVal, 16));
                    WriteToLog(msg);
                    return false;
                }

                //индекс последнего байта последнегй записи
                int lastRecordByteIndex = lastByteIndex - 2;

                //поместим байты записей в отдельный список
                for (int i = firstRecordByteIndex; i <= lastRecordByteIndex; i++)
                    recordsBytesList.Add(inp[i]);

                // т.к. счетчик создается только на итерацию цикла while в сервере опроса,
                // можем закешировать данные на итерацию, тем более суточные и месячные - те же
                // самые данные из req_ud2
                cachedAnsewerBytes = recordsBytesList;

                return true;
            }
            catch (Exception ex)
            {
                WriteToLog("SendREQ_UD2: " + ex.Message);
                return false;
            }
        }

        //Служебный метод, опрашивающий счетчик для всех элементов перечисления Params,
        //и возвращающих ответ в виде строки. Примняется в тестовой утилите драйвера elf.
        //public bool GetAllValues(out string res, bool FCV = true)
        //{
        //    res = "Ошибка";
        //    List<Record> records = new List<Record>();
        //    if (!GetRecordsList(out records, FCV))
        //    {
        //        WriteToLog("GetAllValues: can't split records");
        //        return false;
        //    }

        //    res = "";
        //    foreach (Params p in Enum.GetValues(typeof(Params)))
        //    {
        //        float val = -1f;
        //        string s = "false;";

        //        if (getRecordValueByParam(p, records, out val))
        //            s = val.ToString();

        //        res += String.Format("{0}: {1}\n", p.ToString(), s);
        //    }

        //    return true;
        //}

        #endregion


        public bool ToBcd(int value, ref byte[] byteArr)
        {
            if (value < 0 || value > 99999999)
                return false;

            byte[] ret = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }

            Array.Reverse(ret);
            byteArr = ret;

            return true;
        }

        //используется для вывода в лог
        public string current_secondary_id_str = "серийный номер не определен";
        //выделяет счетчик по серийнику и возвращает признак того что прибор на связи
        public bool SelectBySecondaryId(int factoryNumber)
        {
            current_secondary_id_str = factoryNumber.ToString();

            byte cmd = 0x73;
            byte addr = 0xFD;
            byte CI = 0x52;

            byte[] addrArr = null;
            if (!ToBcd(factoryNumber, ref addrArr))
                return false;

            byte CS = (byte)(cmd + addr + CI + addrArr[3] + addrArr[2] + addrArr[1] + addrArr[0] + 0xFF + 0xFF + 0xFF + 0xFF);

            byte[] cmdArr = { 0x68, 0x0B, 0x0B, 0x68, cmd, addr, CI, addrArr[3], addrArr[2], addrArr[1], addrArr[0],
                            0xFF, 0xFF , 0xFF,0xFF, CS, 0x16 };
            int firstRecordByteIndex = cmdArr.Length + 4 + 3 + 12;


            byte[] inp = new byte[512];
            try
            {
                int readBytes = m_vport.WriteReadData(findPackageSign, cmdArr, ref inp, cmdArr.Length, -1);
                for (int i = inp.Length - 1; i >= 0; i--)
                    if (inp[i] == 0xE5)
                    {
                        WriteToLog("SelectBySecondaryId: выбран счетчик " + current_secondary_id_str, bLogOutBytes);
                        return true;
                    }

                string msg = String.Format("SelectBySecondaryId: в ответе не найден байт подтверждения 0xE5 для счетчика {0}: [{1}]", current_secondary_id_str,
                    String.Join(",", inp));
                WriteToLog(msg);
                return false;

            }
            catch (Exception ex)
            {
                WriteToLog("SelectBySecondaryId: " + ex.Message);
                return false;
            }
        }

        // сбрасывает выделение конкретного счптчика
        public bool SND_NKE(ref bool confirmed)
        {
            byte addr = 0xFF;
            byte cmd = 0x40;
            byte CS = (byte)(cmd + addr);

            byte[] cmdArr = { 0x10, cmd, addr, CS, 0x16 };
            int firstRecordByteIndex = cmdArr.Length + 4 + 3 + 12;

            byte[] inp = new byte[512];
            try
            {
                int readBytes = m_vport.WriteReadData(findPackageSign, cmdArr, ref inp, cmdArr.Length, -1);
                if (readBytes >= 1 && inp[readBytes - 1] == 0xE5)
                    confirmed = true;
                else
                    confirmed = false;

                WriteToLog("SND_NKE: деселекция", bLogOutBytes);

                return true;
            }
            catch (Exception ex)
            {
                WriteToLog("SND_NKE: " + ex.Message);
                return false;
            }

        }
        public bool UnselectAllMeters()
        {
            bool res = false;
            this.SND_NKE(ref res);

            return res;
        }

        List<byte> cachedAnsewerBytes = new List<byte>();
        public bool ReadCurrentValues(List<int> paramCodes, out List<float> values, bool FCV = true)
        {
            values = new List<float>();
            List<Record> records = new List<Record>();
            List<byte> answerBytes = new List<byte>();

            // т.к. счетчик создается только на итерацию цикла while в сервере опроса,
            // можем взять данные из кэшаЖ тем более суточные и месячные - те же
            // самые данные из req_ud2
            if (cachedAnsewerBytes.Count > 0)
            {
                answerBytes = cachedAnsewerBytes;
            }
            else
            {
                if (!SendREQ_UD2(out answerBytes) || answerBytes.Count == 0)
                {
                    WriteToLog("ReadCurrentValues: не получены байты ответа");
                    return false;
                }
            }

            //вывод в лог "сырых" байт, поступивших со счетчика
            if (bLogOutBytes)
            {
                string answBytesStr = String.Format("ReadCurrentValues, response:\n[{0}];", BitConverter.ToString(answerBytes.ToArray()).Replace("-", " "));
                WriteToLog(answBytesStr);
            }

            if (!SplitRecords(answerBytes, ref records) || records.Count == 0)
            {
                WriteToLog("ReadCurrentValues: не удалось разделить запись");
                return false;
            }

            //вывод в лог байт параметров, выделенных программой из "сырого" ответа
            if (bLogOutBytes)
            {
                string recordsStr = String.Empty;
                foreach (Record tR in records)
                    recordsStr += "[" + BitConverter.ToString(tR.dataBytes.ToArray()).Replace("-", " ") + "], ";

                string answBytesStr = String.Format("ReadCurrentValues, records:\n{0};", recordsStr);
                WriteToLog(answBytesStr);
            }

            foreach (int p in paramCodes)
            {
                float tmpVal = -1f;
                values.Add(tmpVal);

                if (!Enum.IsDefined(typeof(Params), p))
                {
                    WriteToLog("ReadCurrentValues не удалось найти в перечислении paramCodes параметр " + p.ToString());
                    continue;
                }

                Params tmpP = (Params)p;

                //не путать с перегруженным аналогом
                if (!getRecordValueByParam(tmpP, records, out tmpVal))
                {
                    WriteToLog("ReadCurrentValues не удалось выполнить getRecordValueByParam для " + tmpP);
                    continue;
                }

                values[values.Count - 1] = tmpVal;
            }

            return true;
        }


        #region Методы интерфейса СО

        private void test()
        {
            byte[] data = { 0x0c, 0x04, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x04, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x14, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x2c, 0x00, 0x00, 0x00, 0x00, 0x0c, 0x3b, 0x00, 0x00, 0x00, 0x00, 0x0b, 0x59, 0x56, 0x23, 0x00, 0x0b, 0x5d, 0x96, 0x22, 0x00, 0x04, 0x6d, 0x30, 0x21, 0xa2, 0x23 };
            List<byte> dataList = data.ToList<byte>();
            List<Record> recordsList = new List<Record>();

            SplitRecords(dataList, ref recordsList);

            float tmpVal = 0;
            getRecordValueByParam(Params.TEMP_INP, recordsList, out tmpVal);
            getRecordValueByParam(Params.TEMP_OUTP, recordsList, out tmpVal);
            getRecordValueByParam(Params.ENERGY, recordsList, out tmpVal);
        }

        public bool OpenLinkCanal()
        {

            //test();
            //return true;

            // выберем счетчик один раз в момент открытия канала
            if (!isMeterSelected)
            {
                UnselectAllMeters();
                isMeterSelected = SelectBySecondaryId((int)m_address);
            }

            return isMeterSelected;
        }

        public int findPackageSign(Queue<byte> queue)
        {
            return 0;
        }

        public bool ReadCurrentValues(ushort param, ushort tarif, ref float recordValue)
        {
            recordValue = -1;
            List<int> prmsList = new List<int>(new int[] { param });
            List<float> vals = new List<float>();



            bool res = ReadCurrentValues(prmsList, out vals);

            if (vals.Count > 0)
            {
                recordValue = vals[0];
            }

            return res;
        }

        public bool ReadDailyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            // прибор не поддерживает суточные
            return ReadCurrentValues(param, tarif, ref recordValue);
        }
        public bool ReadMonthlyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            // прибор не поддерживает месячные
            return ReadCurrentValues(param, tarif, ref recordValue);
        }

        public bool ReadSerialNumber(ref string serial_number)
        {
            return false;
        }


        #endregion

        #region Методы интерфейса неиспользуемые

        public List<byte> GetTypesForCategory(CommonCategory common_category)
        {
            return new List<byte>();
        }

        public bool ReadPowerSlice(DateTime dt_begin, DateTime dt_end, ref List<RecordPowerSlice> listRPS, byte period)
        {
            return false;
        }

        public bool ReadPowerSlice(ref List<SliceDescriptor> sliceUniversalList, DateTime dt_end, SlicePeriod period)
        {
            return false;
        }

        public bool SyncTime(DateTime dt)
        {
            return false;
        }

        public bool ReadSliceArrInitializationDate(ref DateTime lastInitDt)
        {
            return false;
        }

        public bool ReadHalfAnHourValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            return false;
        }

        public bool ReadHourValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            return false;
        }

        public bool ReadDailyValues(uint recordId, ushort param, ushort tarif, ref float recordValue)
        {
            return false;
        }

        #endregion
    }
}
