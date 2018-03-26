using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            this.m_dirWatcher = new FileSystemWatcher(m_path, "*.jpg,*.png,*.gif,*.bmp");
            this.m_dirWatcher.Created += this.OnNewFile;
        }
        public string ExecuteCommand(int commandID, string[] args, out bool result)
        {
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

        public void OnNewFile(object sender, EventArgs e)
        {
            this.m_controller.ExecuteCommand((int)CommandEnum.NewFileCommand, null, out bool res);
        }
    }
}
