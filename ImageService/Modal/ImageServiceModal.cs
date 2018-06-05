using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Infrastructure.Enums;
using Logs.AppConfigObjects;
using Logs.ImageService.Logging;
namespace Logs.Modal
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
        /// a logging service - in the file structure given it wasn't there but
        /// i prefered to add it to here for easier debugging.
        /// </summary>
        private ILoggingService log;
        #endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageServiceModal"/> class.
        /// takes the path to the output folder & thumnail folder
        /// </summary>
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
            catch (Exception)
            {
                this.log.Log("failed to convert thubnail size from app configuration",
                    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
            }
            //try to create the output folder only if it's not exist as an hidden folder
            if (!Directory.Exists(outputFolder))
            {
                try
                {
                    DirectoryInfo di = Directory.CreateDirectory(outputFolder);
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    
                }
                catch (Exception e)
                {
                    this.log.Log("failed to create output dirctory, error is: " + e.Message,
                        ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                }
            }
        }
        /// <summary>
        /// The Function Addes A file to the system and is responsible for the logic
        /// </summary>
        /// <param name="path">The Path of the Image from the file</param>
        /// <param name="result"></param>
        /// <returns>
        /// Indication if the Addition Was Successful
        /// </returns>
        public string AddFile(string path, out bool result)
        {
            //System.Diagnostics.Debugger.Launch();
            DateTime d = GetDateFileCreatedFromImage(path);
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
            //check if the file exsit in the destenation - if it does need to find another name for it
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
            if (File.Exists(sourceFile))
            {
                //check if file is busy
                Task t = new Task(() =>
               {
                   bool stop = false;
                   bool fileLocked;
                   while (!stop)
                   {
                       fileLocked = IsFileLocked(new FileInfo(sourceFile));
                       if (fileLocked == false)
                       {
                           stop = true;
                       }
                       else
                       {
                           Task.Delay(10000);
                       }
                   }
               });
                t.Start();
                t.Wait();
            }
            else
            {
                result = false;
                return "file does not exist";
            }
            //move the file
            string res = this.MoveFile(sourceFile, destFile, out bool success);
            //if move failed - return
            if (!success)
            {
                result = false;
                return res;
            }
            //create a thumbnail copy
            Task <String>t1 = new Task<String>(() =>
            {
                Task.Delay(2000);
                string resultThumbnailCopy =
                this.CreateThumbnailCopy(destFile, Path.GetFileName(destFile), year, month);
                return resultThumbnailCopy;
            });
            t1.Start();
            t1.Wait();
            String task_res = t1.Result;
            if (task_res.Equals(ResultMessgeEnum.Success.ToString()))
            {
                result = true;
            }
            else
            {
                result = false;
            } 
            return task_res;
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

            Task<string> t = new Task<string>(() =>
            {
                Task.Delay(1000);
                try
                {
                    System.IO.File.Move(source, dest);
                    res = ResultMessgeEnum.Success.ToString();

                }
                catch (Exception e) { res = e.ToString(); }
                return res;

            });
            t.Start();
            // t.Wait();
         //   this.log.Log("moving the file to destenation folder, result is: " + t.Result,
            //    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
            result = true;
            res = ResultMessgeEnum.Success.ToString(); 
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
                // System.Diagnostics.Debugger.Launch();
                System.Threading.Thread.Sleep(2000);
                this.log.Log("entered method",ImageService.Logging.Modal.MessageTypeEnum.INFO);
                Image image = Image.FromFile(path);
                this.log.Log("after Image.FromFile", ImageService.Logging.Modal.MessageTypeEnum.INFO);
                Image thumb = image.GetThumbnailImage(this.thumbnailSize, this.thumbnailSize, () => false, IntPtr.Zero);
                this.log.Log("after get tumb", ImageService.Logging.Modal.MessageTypeEnum.INFO);
                string thubnail_path = this.thumbnailpath + "\\\\" + year + "\\\\" + month;
                this.log.Log("after thumb_path", ImageService.Logging.Modal.MessageTypeEnum.INFO);
                if (!Directory.Exists(thubnail_path))
                {
                    System.IO.Directory.CreateDirectory(thubnail_path);//create only if not exist
                }
                this.log.Log("after create directory", ImageService.Logging.Modal.MessageTypeEnum.INFO);
                thumb.Save(thubnail_path + "\\\\" + fileName);
                this.log.Log("after save", ImageService.Logging.Modal.MessageTypeEnum.INFO);
            }
            catch (Exception e)
            {
                
                this.log.Log("error occured while creating thumbnail, error is: " + e.Message.ToString(),
                    ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                return e.ToString();
            }
            return ResultMessgeEnum.Success.ToString();
        }
        /// <summary>
        /// get the date image was created. the link that Dor sent to us didn't work:
        /// -a-when-out-find-i-can-https://stackoverflow.com/questions/180030/how vista-on-running-sharp-c-in-taken-actually-was-picture
        /// after talking with him - he allows me to use this function.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>return a Datetime object about the creation of the folder </returns>
        private DateTime GetDateFileCreatedFromImage(string path)
        {
            DateTime d;
            try
            {
                d = File.GetCreationTime(path);
            } catch (Exception)
            {
                d = DateTime.Now;
                this.log.Log("error in GetDateFileCreatedFromImage, putting the date and time of now",
                    ImageService.Logging.Modal.MessageTypeEnum.INFO);
            }
            return d;
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
            int count = 0;

            string fileNameOnly = fileName;
            string extension = fileExtension;
            string path = p;
            string newFullPath = newPath;

            while (File.Exists(@newFullPath))
            {
                ++count;
                newFullPath = System.IO.Path.Combine(Path.GetDirectoryName(newPath),
                    Path.GetFileNameWithoutExtension(path) + "(" + count + ")" +
                    extension);
            }
            return count;
        }
        /// <summary>
        /// Determines whether the file is locked.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   <c>true</c> if is file locked and otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        /// <summary>
        /// Gets the appconfig as json.
        /// </summary>
        /// <returns>appconfig as json string</returns>
        public String GetAppconfig()
        {
            ImageServiceAppConfigItem configuration;
             configuration = new ImageServiceAppConfigItem(ConfigurationManager.AppSettings.Get("OutputDir") as String,
                ConfigurationManager.AppSettings.Get("Handler").ToString(),
                ConfigurationManager.AppSettings.Get("SourceName").ToString(),
                ConfigurationManager.AppSettings.Get("LogName").ToString(),
                Convert.ToInt32((ConfigurationManager.AppSettings.Get("ThumbnailSize")).ToString()));
            
            return configuration.ToJSON();
        }
        public String LogCommand()
        {
            return null;
        }
    }
}
