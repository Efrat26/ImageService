using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using ImageService.ImageService.Infrastructure.Enums;
namespace ImageService.Modal
{
    public class ImageServiceModal : IImageServiceModal
    {
        #region Members        
        /// <summary>
        /// The output folder path
        /// </summary>
        private string outputFolder;     
        /// <summary>
        /// thumbnail size
        /// </summary>
        private int thumbnailSize;
        /// <summary>
        /// path to the thumbnail folder
        /// </summary>
        private string thumbnailpath;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageServiceModal"/> class.
        /// takes the path to the output folder & thumnail folder
        /// </summary>
        public ImageServiceModal()
        {
            this.outputFolder = ConfigurationManager.AppSettings.Get("OutputDir");
            this.thumbnailpath = this.outputFolder + "\\\\" + "Thumbnails";
            //try to parse the data
            try
            {
                this.thumbnailSize = Int32.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
            } catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }
        /// <summary>
        /// The Function Addes A file to the system
        /// </summary>
        /// <param name="path">The Path of the Image from the file</param>
        /// <param name="result"></param>
        /// <returns>
        /// Indication if the Addition Was Successful
        /// </returns>
        public string AddFile(string path, out bool result)
        {
            
            DateTime d = GetDateTakenFromImage(path);
            int year = this.GetYearAsNumber(d);
            int month = this.GetMonthAsNumber(d);
            string newPath = this.outputFolder + "\\\\" + year;
            //creates the directory only if not exist
            System.IO.Directory.CreateDirectory(newPath);
            newPath = newPath + "\\\\" + month;
            string fileName = Path.GetFileName(path);
            //prepare the source and target string for the move file command
            string sourceFile = System.IO.Path.Combine(Path.GetDirectoryName(path), fileName);
            string destFile = System.IO.Path.Combine(newPath, fileName);
            //move the file
            string res = this.MoveFile(sourceFile, destFile, out bool success);
            if (!success)
            {
                result = false;
                return res;
            }
            string resultThumbnailCopy =
                this.CreateThumbnailCopy(newPath + "\\\\" + fileName, fileName, year, month);
               if (resultThumbnailCopy.Equals(ResultMessgeEnum.Success.ToString()))
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return resultThumbnailCopy; 
        }
        /// <summary>
        /// Moves the file from source folder to target folder.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        /// <param name="result">if set to true if was succesful and false otherwise.</param>
        /// <returns>a string with the error message or success message</returns>
        public string MoveFile(string source, string dest, out bool result)
        {
            string res;
            try
            {
                System.IO.File.Move(source, dest);
                res = ResultMessgeEnum.Success.ToString();
            }
            catch (Exception e)
            {
                result = false;
                return res = e.ToString();
            }
            result = true;
            return res;
        }
        /// <summary>
        /// Creates the thumbnail copy.
        /// </summary>
        /// <param name="path">path to the file</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <returns>a string with the error message or success message</returns>
        private string CreateThumbnailCopy(string path, string fileName, int year, int month)
        {
            try
            {
                Image image = Image.FromFile(path);
                Image thumb = image.GetThumbnailImage(this.thumbnailSize, this.thumbnailSize, () => false, IntPtr.Zero);
                string thubnail_path = this.thumbnailpath + "\\\\" + year + "\\\\" + month;
                System.IO.Directory.CreateDirectory(thubnail_path);//create only if not exist
                thumb.Save(thubnail_path + "\\\\" + fileName);
            } catch (Exception e)
            {
                return e.ToString();
            }
            return ResultMessgeEnum.Success.ToString();
        }
        /// <summary>
        /// get the date image was created .
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>return a Datetime object about the creation of the folder </returns>
        private static DateTime GetDateTakenFromImage(string path)
        {
            return File.GetCreationTime(path);
        }
        /// <summary>
        /// returns the month as number.
        /// </summary>
        /// <param name="d">The a datetime object, contains information about the creation date
        /// of the image</param>
        /// <returns>month when the image was taken as number</returns>
        private int GetMonthAsNumber(DateTime d)
        {
            string sMonth = d.Month.ToString();
            return Int32.Parse(sMonth);
        }
        /// <summary>
        /// Gets the year as number.
        /// </summary>
        /// <param name="d">The DateTime object, contains information about the creation date
        /// of the image</param>
        /// <returns>return month as number</returns>
        private int GetYearAsNumber(DateTime d)
        {
            string sYear = d.Year.ToString();
            return Int32.Parse(sYear);
        }
        #endregion
    }
}
