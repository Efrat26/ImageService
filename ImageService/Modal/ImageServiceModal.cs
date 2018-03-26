using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;              // The Size Of The Thumbnail Size
        public ImageServiceModal()
        {
            this.m_OutputFolder = ConfigurationManager.AppSettings.Get("OutputDir");
            this.m_thumbnailSize = Int32.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
        }
        public string AddFile(string path, out bool result)
        {
            DateTime d = GetDateTakenFromImage(path);
            int year = this.GetYearAsNumber(d);
            int month = this.GetMonthAsNumber(d);
            string newPath = this.m_OutputFolder + '\\' + year;
            System.IO.Directory.CreateDirectory(newPath);//create only if not exist
            newPath = newPath + '\\' + month;
            System.IO.Directory.CreateDirectory(newPath);
            if (this.CreateFile(newPath))
            {
                //create also in thumbnail
                this.CreateThumbnailCopy(path);
                result = true;
                return "success";
            }
            result = false;
            return "error";
        }
        public string MoveFile(string path, out bool result)
        {
            result = true;
            return null;
        }
        //create a copy in thumbnail
        private bool CreateThumbnailCopy(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(this.m_thumbnailSize);
                return true;
            }
        }
        //create file int the folder
        private bool CreateFile(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(path))
                {
                    for (byte i = 0; i < 100; i++)
                    {
                        fs.WriteByte(i);
                    }
                }
                return true;
            }
            return false;
        }
        //get the date image was created
        private static DateTime GetDateTakenFromImage(string path)
        {
            return File.GetCreationTime(path);
        }
        //return month as number
        private int GetMonthAsNumber(DateTime d)
        {
            string sMonth = DateTime.Now.ToString("MM");
            return Int32.Parse(sMonth);
        }
        //return month as number
        private int GetYearAsNumber(DateTime d)
        {
            string sYear = DateTime.Now.ToString("YY");
            return Int32.Parse(sYear);
        }
        //check if directory exist
        private bool CheckIfDirExist(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
