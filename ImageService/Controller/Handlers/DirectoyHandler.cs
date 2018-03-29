using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.ImageService.Logging.Modal;
using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageService.ImageService.Infrastructure.Enums;

namespace ImageService.Controller.Handlers
{
    class DirectoyHandler : IDirectoryHandler
    {
        #region Members        
        /// <summary>
        /// The Image Processing Controller
        /// </summary>
        private IImageController m_controller;
        /// <summary>
        /// a logger object
        /// </summary>
        private ILoggingService m_logging;
        /// <summary>
        /// The Watcher of the Directory
        /// </summary>
        private FileSystemWatcher m_dirWatcher;
        /// <summary>
        /// The Path of directory
        /// </summary>
        private string m_path;
        #endregion
        // The Event That Notifies that the Directory is being closed
        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoyHandler"/> class.
        /// </summary>
        /// <param name="path">The path to the directory it takes care.</param>
        /// <param name="m">imagecontroller object</param>
        /// <param name="l">a logging service</param>
        public DirectoyHandler(string path, IImageController m, ILoggingService l)
        {
            this.m_path = path;
            this.m_controller = m;
            this.m_logging = l;
            this.m_dirWatcher = new FileSystemWatcher();
            this.m_logging.Log("Hello frm handler", ImageService.Logging.Modal.MessageTypeEnum.INFO);
            this.InitializeWatcher();
        }
        /// <summary>
        /// Initializes the watcher - signs to the OnCreated event and enable rising events
        /// </summary>
        private void InitializeWatcher()
        {
            this.m_dirWatcher.Path = this.m_path;
            this.m_dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            this.m_dirWatcher.Created += this.OnNewFile;
            this.m_dirWatcher.EnableRaisingEvents = true;
            this.m_logging.Log("in initialize watcher of handler", MessageTypeEnum.INFO);
        }
        /// <summary>
        /// Executes the command specified by command ID.
        /// </summary>
        /// <param name="commandID">The command identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="result">if set to true if the operation was
        /// successful and false otherwise</param>
        /// <returns>
        /// a string contains error message (if error occured) or a
        /// success message if it done without errors
        /// </returns>
        public string ExecuteCommand(int commandID, string[] args, out bool result)
        {
            this.m_logging.Log("in execute command of server", MessageTypeEnum.INFO);
            string resVal = m_controller.ExecuteCommand(commandID, args, out bool res);
            if (resVal == "success")
            {
                result = true;
                this.m_logging.Log(commandID + " " + resVal, ImageService.Logging.Modal.MessageTypeEnum.INFO);
            }
            else
            {
                result = false;
                this.m_logging.Log(commandID + " " + resVal, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
            }
            return resVal;
        }
        /// <summary>
        /// Called when there was a command.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CommandRecievedEventArgs" /> instance containing the event data.</param>
        public void OnCommand(object sender, CommandRecievedEventArgs e)
        {
            this.m_logging.Log("in on command of handler", MessageTypeEnum.INFO);
            // System.Diagnostics.Debugger.Launch();
            if (e.CommandID == (int)CommandEnum.CloseCommand)
            {
                this.m_dirWatcher.Changed -= this.OnNewFile;
                this.m_dirWatcher.EnableRaisingEvents = false;
                this.m_dirWatcher.Dispose();
                this.m_logging.Log("file system watch closed", MessageTypeEnum.INFO);
            }
            else
            {
                if (e.RequestDirPath == this.m_path)
                {
                    if (e.CommandID == (int)CommandEnum.CloseCommand)
                    {
                        this.m_dirWatcher.Dispose();
                        this.DirectoryClose?.Invoke(this, new DirectoryCloseEventArgs(this.m_path, "directory closed"));
                    }
                    else
                    {
                        string res = this.m_controller.ExecuteCommand(e.CommandID, e.Args, out bool result);
                        if (ResultMessgeEnum.Success.Equals(res))
                        {
                            this.m_logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                        }
                        else
                        {
                            this.m_logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Called when there's a new file request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs" /> instance containing the event data.</param>
        public void OnNewFile(object sender, FileSystemEventArgs e)
        {
            string[] filters = { ".jpg", ".png", ".gif", ".bmp" };
            string strFileExt = Path.GetExtension(e.FullPath);
            this.m_logging.Log("in handler - event on new file reciveced for before entering the condition" + e.Name, ImageService.Logging.Modal.MessageTypeEnum.INFO);
            // System.Diagnostics.Debugger.Launch();
            //enter here only if the file contains one of the extensions
            if (filters.Contains(strFileExt))
            {
                string[] args = { e.FullPath, e.Name };
                //bool result = true;
                this.m_controller.ExecuteCommand((int)CommandEnum.NewFileCommand, args, out bool result);
                if (result)
                {
                    this.m_logging.Log("in handler - event on new file reciveced for" + e.Name, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                }
                else
                {
                    this.m_logging.Log("in handler - event on new file reciveced for" + e.Name, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                }

            }
        }
    }
}
