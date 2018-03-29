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

namespace ImageService.Controller.Handlers
{
    class DirectoyHandler : IDirectoryHandler
    {
        #region Members
        private IImageController m_controller;              // The Image Processing Controller
        private ILoggingService m_logging;
        private FileSystemWatcher m_dirWatcher;             // The Watcher of the Dir
        private string m_path;                              // The Path of directory
        #endregion
        // The Event That Notifies that the Directory is being closed
        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;
        public DirectoyHandler(string path, IImageController m, ILoggingService l)
        {

            this.m_path = path;
            this.m_controller = m;
            this.m_logging = l;
            //create new file system watcher to monitor image files
            //
            // this.m_dirWatcher = new FileSystemWatcher(m_path, "*.jpg");
            this.m_dirWatcher = new FileSystemWatcher();
            this.m_logging.Log("Hello frm handler", ImageService.Logging.Modal.MessageTypeEnum.INFO);
            this.InitializeWatcher();
        }
        private void InitializeWatcher()
        {
            this.m_dirWatcher.Path = this.m_path;
            this.m_dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //this.m_dirWatcher.NotifyFilter = NotifyFilters.CreationTime;
            //"*,.jpg,*.png,*.gif,*.bmp"
            //this.m_dirWatcher.Changed += new FileSystemEventHandler(OnNewFile);
            this.m_dirWatcher.Changed += new FileSystemEventHandler(this.OnNewFile);
            this.m_dirWatcher.Created += new FileSystemEventHandler(this.OnNewFile);
           // this.m_dirWatcher.Deleted += new FileSystemEventHandler(this.OnNewFile);
            //this.m_dirWatcher.Renamed += new RenamedEventHandler(this.OnNewFile);
            this.m_dirWatcher.EnableRaisingEvents = true;
            //this.m_dirWatcher.Created += this.OnNewFile;
            this.m_logging.Log("in initialize watcher of handler", MessageTypeEnum.INFO);
        }
        public string ExecuteCommand(int commandID, string[] args, out bool result)
        {
            this.m_logging.Log("in execute command of server", MessageTypeEnum.INFO);
            string resVal =  m_controller.ExecuteCommand(commandID, args, out bool res);
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
        public void OnCommand(object sender, CommandRecievedEventArgs e)
        {
            this.m_logging.Log("in on command of handler", MessageTypeEnum.INFO);
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
                    if (res == "error")
                    {
                        this.m_logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                    }
                    else
                    {
                        this.m_logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                    }
                    }
            }
        }

        public void OnNewFile(object sender, FileSystemEventArgs e)
        {
            string[] filters = { ".jpg", ".png", ".gif", ".bmp"};
            string strFileExt = Path.GetExtension(e.FullPath);
            this.m_logging.Log("in handler - event on new file reciveced for before entering the condition" + e.Name, ImageService.Logging.Modal.MessageTypeEnum.INFO);
            System.Diagnostics.Debugger.Launch();
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
            //this.m_controller.ExecuteCommand((int)CommandEnum.NewFileCommand, null, out bool res);
        }
    }
}
