using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Drivers.LibMeter;


namespace Drivers.LibMBus
{
    public class MBusDriver : CMeter
    {
        public struct Record
        {
            public byte DIF;
            public List<byte> DIFEs;

            public byte VIF;
            public List<byte> VIFEs;

            public List<byte> dataBytes;

            public RecordDataType recordType;
        }

        public enum RecordDataType
        {
            NO_DATA = 0,
            INTEGER = 1,
            REAL = 2,
            BCD = 3,
            VARIABLE_LENGTH = 4,
            SELECTION_FOR_READOUT = 5,
            SPECIAL_FUNСTIONS = 6
        }

        // из скольких байт состоят полезные данные и какого они типа (INTEGER, но разного размера)
        // или особенный тип
        public int GetLengthAndTypeFromDIF(byte DIF, out RecordDataType type)
        {
            int data = DIF & 0x0F; //00001111b
            switch (data)
            {
                case 0:
                    {
                        type = RecordDataType.NO_DATA;
                        return 0;
                    }
                case 1:
                    {
                        type = RecordDataType.INTEGER;
                        return 1;
                    }
                case 2:
                    {
                        type = RecordDataType.INTEGER;
                        return 2;
                    }
                case 3:
                    {
                        type = RecordDataType.INTEGER;
                        return 3;
                    }
                case 4:
                    {
                        type = RecordDataType.INTEGER;
                        return 4;
                    }
                case 5:
                    {
                        WriteToLog("getLengthAndTypeFromDIF: 5, real");
                        type = RecordDataType.REAL;
                        return 4;
                    }
                case 6:
                    {
                        type = RecordDataType.INTEGER;
                        return 6;
                    }
                case 7:
                    {
                        type = RecordDataType.INTEGER;
                        return 8;
                    }
                case 8:
                    {
                        //selection for readout
                        WriteToLog("getLengthAndTypeFromDIF: 8, selection for readout");
                        type = RecordDataType.SELECTION_FOR_READOUT;
                        return 0;
                    }
                case 9:
                    {
                        type = RecordDataType.BCD;
                        return 1;
                    }
                case 0x0A:
                    {
                        type = RecordDataType.BCD;
                        return 2;
                    }
                case 0x0B:
                    {
                        type = RecordDataType.BCD;
                        return 3;
                    }
                case 0x0C:
                    {
                        type = RecordDataType.BCD;
                        return 4;
                    }
                case 0x0D:
                    {
                        WriteToLog("getLengthAndTypeFromDIF: 13, variable length");
                        type = RecordDataType.VARIABLE_LENGTH;
                        return -1;
                    }
                case 0x0E:
                    {
                        WriteToLog("getLengthAndTypeFromDIF: 14, not defined");
                        type = RecordDataType.NO_DATA;
                        return -1;
                    }
                case 0x0F:
                    {
                        WriteToLog("getLengthAndTypeFromDIF: 15, special functions");
                        type = RecordDataType.SPECIAL_FUNСTIONS;
                        return -1;
                    }
                default:
                    {
                        type = RecordDataType.NO_DATA;
                        return -1;
                    }
            }
        }

        //возвращает true если установлен extension bit, позволяет опрелелить, есть ли DIFE/VIFE
        private bool hasExtension(byte b)
        {
            byte EXTENSION_BIT_MASK = Convert.ToByte("10000000", 2);
            int extensionBit = (b & EXTENSION_BIT_MASK) >> 7;
            if (extensionBit == 1)
                return true;
            else
                return false;
        }

        public bool SplitRecords(List<byte> recordsBytes, ref List<Record> recordsList)
        {
            recordsList = new List<Record>();
            if (recordsBytes.Count == 0) return false;

            bool doStop = false;
            int index = 0;

            //переберем записи
            while (!doStop)
            {
                Record tmpRec = new Record();
                tmpRec.DIFEs = new List<byte>();
                tmpRec.VIFEs = new List<byte>();
                tmpRec.dataBytes = new List<byte>();

                tmpRec.DIF = recordsBytes[index];

                //определим длину и тип данных
                int dataLength = GetLengthAndTypeFromDIF(tmpRec.DIF, out tmpRec.recordType);

                if (tmpRec.recordType == RecordDataType.SPECIAL_FUNСTIONS)
                {
                    // к примеру, в драйвере СоноДанфос, признак 0x0F означает, что интересующие 
                    // нас данные уже закончились и далее идут данные от производителя произвольного
                    // размера. Поэтому проигнорируем их.
                    break;
                }

                if (hasExtension(tmpRec.DIF))
                {
                    //переход к байту DIFE
                    index++;
                    byte DIFE = recordsBytes[index];
                    tmpRec.DIFEs.Add(DIFE);

                    while (hasExtension(DIFE))
                    {
                        //перейдем к следующему DIFE
                        index++;
                        DIFE = recordsBytes[index];
                        tmpRec.DIFEs.Add(DIFE);
                    }
                }

                //переход к VIF
                index++;
                tmpRec.VIF = recordsBytes[index];

                //проверим на наличие специального VIF, после которого следует ASCII строка
                //if (tmpRec.VIF == Convert.ToByte("11111100", 2))
                //{
                //    index++;
                //    int str_length = recordsBytes[index];
                //    index += str_length;
                //}

                if (hasExtension(tmpRec.VIF))
                {
                    //переход к VIFE
                    index++;
                    byte VIFE = recordsBytes[index];
                    tmpRec.VIFEs.Add(VIFE);

                    while (hasExtension(VIFE))
                    {
                        //перейдем к следующему VIFE
                        index++;
                        VIFE = recordsBytes[index];
                        tmpRec.VIFEs.Add(VIFE);
                    }
                }

                //переход к первому байту данных
                index++;
                int dataCnt = 0;
                while (dataCnt < dataLength)
                {
                    tmpRec.dataBytes.Add(recordsBytes[index]);
                    index++;
                    dataCnt++;
                }

                recordsList.Add(tmpRec);
                if (index >= recordsBytes.Count - 1) doStop = true;
            }

            return true;
        }

        #region Расчет контрольной суммы
        // CRC-8 for Dallas iButton products from Maxim/Dallas AP Note 27
        readonly byte[] crc8Table = new byte[]
        {
            0x00, 0x5E, 0xBC, 0xE2, 0x61, 0x3F, 0xDD, 0x83,
            0xC2, 0x9C, 0x7E, 0x20, 0xA3, 0xFD, 0x1F, 0x41,
            0x9D, 0xC3, 0x21, 0x7F, 0xFC, 0xA2, 0x40, 0x1E,
            0x5F, 0x01, 0xE3, 0xBD, 0x3E, 0x60, 0x82, 0xDC,
            0x23, 0x7D, 0x9F, 0xC1, 0x42, 0x1C, 0xFE, 0xA0,
            0xE1, 0xBF, 0x5D, 0x03, 0x80, 0xDE, 0x3C, 0x62,
            0xBE, 0xE0, 0x02, 0x5C, 0xDF, 0x81, 0x63, 0x3D,
            0x7C, 0x22, 0xC0, 0x9E, 0x1D, 0x43, 0xA1, 0xFF,
            0x46, 0x18, 0xFA, 0xA4, 0x27, 0x79, 0x9B, 0xC5,
            0x84, 0xDA, 0x38, 0x66, 0xE5, 0xBB, 0x59, 0x07,
            0xDB, 0x85, 0x67, 0x39, 0xBA, 0xE4, 0x06, 0x58,
            0x19, 0x47, 0xA5, 0xFB, 0x78, 0x26, 0xC4, 0x9A,
            0x65, 0x3B, 0xD9, 0x87, 0x04, 0x5A, 0xB8, 0xE6,
            0xA7, 0xF9, 0x1B, 0x45, 0xC6, 0x98, 0x7A, 0x24,
            0xF8, 0xA6, 0x44, 0x1A, 0x99, 0xC7, 0x25, 0x7B,
            0x3A, 0x64, 0x86, 0xD8, 0x5B, 0x05, 0xE7, 0xB9,
            0x8C, 0xD2, 0x30, 0x6E, 0xED, 0xB3, 0x51, 0x0F,
            0x4E, 0x10, 0xF2, 0xAC, 0x2F, 0x71, 0x93, 0xCD,
            0x11, 0x4F, 0xAD, 0xF3, 0x70, 0x2E, 0xCC, 0x92,
            0xD3, 0x8D, 0x6F, 0x31, 0xB2, 0xEC, 0x0E, 0x50,
            0xAF, 0xF1, 0x13, 0x4D, 0xCE, 0x90, 0x72, 0x2C,
            0x6D, 0x33, 0xD1, 0x8F, 0x0C, 0x52, 0xB0, 0xEE,
            0x32, 0x6C, 0x8E, 0xD0, 0x53, 0x0D, 0xEF, 0xB1,
            0xF0, 0xAE, 0x4C, 0x12, 0x91, 0xCF, 0x2D, 0x73,
            0xCA, 0x94, 0x76, 0x28, 0xAB, 0xF5, 0x17, 0x49,
            0x08, 0x56, 0xB4, 0xEA, 0x69, 0x37, 0xD5, 0x8B,
            0x57, 0x09, 0xEB, 0xB5, 0x36, 0x68, 0x8A, 0xD4,
            0x95, 0xCB, 0x29, 0x77, 0xF4, 0xAA, 0x48, 0x16,
            0xE9, 0xB7, 0x55, 0x0B, 0x88, 0xD6, 0x34, 0x6A,
            0x2B, 0x75, 0x97, 0xC9, 0x4A, 0x14, 0xF6, 0xA8,
            0x74, 0x2A, 0xC8, 0x96, 0x15, 0x4B, 0xA9, 0xF7,
            0xB6, 0xE8, 0x0A, 0x54, 0xD7, 0x89, 0x6B, 0x35
        };

        // TODO: во-первых, заменить на функцию из библиотеки с CRC
        // во-вторых, проверить как считается CRC в экземплярах драйвера
        public byte CRC8(byte[] bytes, int len)
        {
            byte crc = 0;
            for (var i = 0; i < len; i++)
                crc = crc8Table[crc ^ bytes[i]];

            //byte[] crcArr = new byte[1];
            // crcArr[0] = crc;
            //MessageBox.Show(BitConverter.ToString(crcArr));
            return crc;
        }

        #endregion

    }
}
