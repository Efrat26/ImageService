﻿using ImageService.ImageService.Infrastructure.Enums;
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
        }
        public string ExecuteCommand(int commandID, string[] args, out bool result)
        {
            throw new NotImplementedException();
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
                }


            }
        }
    }
}
