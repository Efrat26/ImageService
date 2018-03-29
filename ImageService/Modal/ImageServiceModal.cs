using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members
        private string m_OutputFolder;            // The Output Folder
        private int m_thumbnailSize;              // The Size Of The Thumbnail Size
        private string m_thumbnailpath;
        public ImageServiceModal()
        {
            this.m_OutputFolder = ConfigurationManager.AppSettings.Get("OutputDir");
            this.m_thumbnailpath = this.m_OutputFolder + "\\\\" + "Thumbnails";
            try
            {
                this.m_thumbnailSize = Int32.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        public string AddFile(string path, out bool result)
        {
            
            DateTime d = GetDateTakenFromImage(path);
            int year = this.GetYearAsNumber(d);
            int month = this.GetMonthAsNumber(d);
            string newPath = this.m_OutputFolder + "\\\\" + year;
            System.IO.Directory.CreateDirectory(newPath);//create only if not exist
            newPath = newPath + "\\\\" + month;
            string fileName = Path.GetFileName(path);
            //newPath = newPath + '\\' + file;


            string sourceFile = System.IO.Path.Combine(Path.GetDirectoryName(path), fileName);
            string destFile = System.IO.Path.Combine(newPath, fileName);

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            string res;
            try
            {
                System.IO.File.Copy(sourceFile, destFile, true);
                res = "suucess";
            } catch (Exception e)
            {
                result = false;
                return res = e.ToString();
                
            }
                System.Diagnostics.Debugger.Launch();
                try
                {
                    Image image = Image.FromFile(newPath + "\\\\" + fileName);
                    Image thumb = image.GetThumbnailImage(this.m_thumbnailSize, this.m_thumbnailSize, () => false, IntPtr.Zero);
                    string thubnail_path = this.m_thumbnailpath + "\\\\" + year + "\\\\" + month;
                    System.IO.Directory.CreateDirectory(thubnail_path);//create only if not exist
                    thumb.Save(thubnail_path + "\\\\" + fileName);
                    
                } catch (Exception e)
                {
                result = false;
                return res =e.ToString();
                   
                }
            
            result = true;
            return res  = "success";

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
            string sMonth = d.Month.ToString();
            return Int32.Parse(sMonth);
        }
        //return month as number
        private int GetYearAsNumber(DateTime d)
        {
            string sYear = d.Year.ToString();
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
