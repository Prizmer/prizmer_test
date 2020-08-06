using Drivers.LibMeter;
using PollingLibraries.LibPorts;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;

namespace Drivers.KaratDanfosDriver
{
    public class IndivDriver : CMeter, IMeter
    {
        const string FTP_PATH = "/report/";
        const string FTP_FILENAME = "current2.csv";
        const string INDIV_DIR_NAME = "indiv";

        // актуальный файл с текущими показаниями, скаченный с устройства
        FileInfo currentFileInfo = null;

        NetworkCredential ftpCredentials = null;
        string ftpHost = "";
        string ftpFileCurrentPath = "";

        // идентификатор концентратора
        string indivId = "";

        // TODO: сделать интерфейс получения информации из драйвера - по параметрам
        // и по паролю

        public void Init(uint address, string pass, VirtualPort data_vport)
        {
            // ожидаемый формат пароля: ftp;admin;admin;1234 - номер прибора
            string[] passwordParts = pass.Split(';');
            if (passwordParts.Length < 4)
                return;
                // TODO: ловить исключения в драйверах
                //throw new Exception("Не возможно инициализировать драйвер, ожидаемый формат пароля ftp;admin;admin;1234");

            ftpCredentials = new NetworkCredential(passwordParts[1], passwordParts[2]);
            ftpFileCurrentPath = FTP_PATH + FTP_FILENAME;

            // TODO: получать из data_vport
            ftpHost = "192.168.0.1";

            indivId = passwordParts[3];
        }

        /**
         * Проверяет наличие актуального файла с текущими показаниями, а при его отсутствии
         * скачивает файл по FTP
         */
        public bool OpenLinkCanal()
        {
            // обнулим информацию о файле
            currentFileInfo = null;

            string fileName = string.Format("/{0}/{1}_current.csv", INDIV_DIR_NAME, indivId);
            FileInfo fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + fileName);

            if (!File.Exists(fi.FullName) || fi.CreationTime.Date != DateTime.Now.Date)
            {
                Directory.CreateDirectory(fi.DirectoryName);
                // скачиваем файл по ftp
                DownloadFileFTP(ftpCredentials, ftpHost, ftpFileCurrentPath, fi.FullName);
            }

            if (File.Exists(fi.FullName))
            {
                currentFileInfo = fi;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ReadCurrentValues(ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadDailyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
        {
            throw new NotImplementedException();
        }

        public bool ReadSerialNumber(ref string serial_number)
        {
            serial_number = "Прибор не позволяет считать серийный номер";
            return true;
        }

        private void DownloadFileFTP(NetworkCredential ftpCredentials, string ftpHost, 
            string ftpFilePath, string inputFilePath)
        {
            string ftpfullpath = "ftp://" + ftpHost + ftpFilePath;

            using (WebClient request = new WebClient())
            {
                request.Credentials = ftpCredentials;
                byte[] fileData = request.DownloadData(ftpfullpath);

                using (FileStream file = File.Create(inputFilePath))
                {
                    file.Write(fileData, 0, fileData.Length);
                    file.Close();
                }
            }
        }


        #region Не используемые

        public bool ReadDailyValues(uint recordId, ushort param, ushort tarif, ref float recordValue)
        {
            return false;
        }

        public bool ReadMonthlyValues(DateTime dt, ushort param, ushort tarif, ref float recordValue)
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
