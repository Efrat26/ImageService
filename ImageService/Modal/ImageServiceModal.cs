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
using ImageService.ImageService.Logging;

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
        private ILoggingService log;
        public ImageServiceModal(ILoggingService l)
        {
            this.log = l;
            this.outputFolder = ConfigurationManager.AppSettings.Get("OutputDir");
            this.thumbnailpath = this.outputFolder + "\\\\" + "Thumbnails";
            //try to parse the data
            try
            {
                this.thumbnailSize = Int32.Parse(ConfigurationManager.AppSettings.Get("ThumbnailSize"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception e)
            {
                this.log.Log("failed to create output dirctory, error is: " + e.Message,
                    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
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
            if (!Directory.Exists(newPath))
            {
                System.IO.Directory.CreateDirectory(newPath);
            }

            newPath = newPath + "\\\\" + month;
            if (!Directory.Exists(newPath))
            {
                System.IO.Directory.CreateDirectory(newPath);
            }

            string fileName = Path.GetFileName(path);
          
            //prepare the source and target string for the move file command
            string sourceFile = System.IO.Path.Combine(Path.GetDirectoryName(path), fileName);
            string destFile = System.IO.Path.Combine(newPath, fileName);
            //check if the file exsit in the destenation
            if (File.Exists(destFile))
            {
                //System.Diagnostics.Debugger.Launch();
                int extensionNum = this.FindFileExtension(sourceFile,
                    fileName, Path.GetExtension(sourceFile), destFile);
                //create new name for dest file
                destFile = System.IO.Path.Combine(newPath,
                    Path.GetFileNameWithoutExtension(path) + "(" + extensionNum + ")" +
                    Path.GetExtension(sourceFile));
            }
            //move the file
            string res = this.MoveFile(sourceFile, destFile, out bool success);
            if (!success)
            {
                result = false;
                return res;
            }
            string resultThumbnailCopy =
                this.CreateThumbnailCopy(destFile, Path.GetFileName(destFile), year, month);
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
            string res = null;
            if (File.Exists(source))
            {
                Task<string> t = new Task<string>(() =>
                {
                    try
                    {
                        System.IO.File.Move(source, dest);
                        res = ResultMessgeEnum.Success.ToString();

                    }
                    catch (Exception e) { res = e.ToString();} return res;

                });
                t.Start();
                t.Wait();
                this.log.Log("error occured while moving the file, error is: " + t.Result,
                    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                result = true;
                res = ResultMessgeEnum.Success.ToString();
            }
            else
            {
                result = false;
                res = "file does not exist";
            }

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
                if (!Directory.Exists(thubnail_path))
                {
                    System.IO.Directory.CreateDirectory(thubnail_path);//create only if not exist
                }
                thumb.Save(thubnail_path + "\\\\" + fileName);
            }
            catch (Exception e)
            {
                this.log.Log("error occured while creating thumbnail, error is: " + e.Message,
                    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
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
        /// <summary>
        /// this function finds a different name to the file if the file is already exsiting
        /// by adding a number to the file name. it finds the least number that makes the 
        /// file to not be already existed in the system. 
        /// i used this code: https://stackoverflow.com/questions/13049732/automatically-rename-a-file-if-it-already-exists-in-windows-way
        /// </summary>
        /// <param name="p">The path (source path)</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="newPath">The destenation path.</param>
        /// <returns> the number to add to the end of the file name </returns>
        private int FindFileExtension(string p, string fileName, string fileExtension, string newPath)
        {
            int count = 1;

            string fileNameOnly = fileName;
            string extension = fileExtension;
            string path = p;
            string newFullPath = newPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return count;
        }
        #endregion
    }
}
