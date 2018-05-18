﻿using Infrastructure.Enums;
using Logs.ImageService.Logging;
using Logs.ImageService.Logging.Modal;
using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Logs.Controller.Handlers
{
    class DirectoyHandler : IDirectoryHandler
    {
        #region Members        
        /// <summary>
        /// The Image Processing Controller
        /// </summary>
        private IImageController controller;
        /// <summary>
        /// a logger object
        /// </summary>
        private ILoggingService logging;
        /// <summary>
        /// The Watcher of the Directory
        /// </summary>
        private FileSystemWatcher dirWatcher;
        /// <summary>
        /// The Path of directory
        /// </summary>
        private string path;
        #endregion
        // The Event That Notifies that the Directory is being closed
        public event EventHandler<DirectoryCloseEventArgs> DirectoryClose;

        public String FullPath { get { return this.path; } set {path = value; } }
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoyHandler"/> class.
        /// </summary>
        /// <param name="p">The path to the directory it takes care.</param>
        /// <param name="m">imagecontroller object</param>
        /// <param name="l">a logging service</param>
        public DirectoyHandler(string p, IImageController m, ILoggingService l)
        {
            this.path = p;
            this.controller = m;
            this.logging = l;
            this.dirWatcher = new FileSystemWatcher();
            //this.logging.Log("Hello frm handler", ImageService.Logging.Modal.MessageTypeEnum.INFO);
            this.InitializeWatcher();
            this.DirectoryClose += this.controller.OnCloseOfService;
        }
        /// <summary>
        /// Initializes the watcher - signs to the OnCreated event and enable rising events
        /// </summary>
        private void InitializeWatcher()
        {
            this.dirWatcher.Path = this.path;
            this.dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            this.dirWatcher.Created += this.OnNewFile;
            this.dirWatcher.EnableRaisingEvents = true;
            //this.logging.Log("in initialize watcher of handler", MessageTypeEnum.INFO);
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
            this.logging.Log("in execute command of handler, commandID: " + commandID, MessageTypeEnum.INFO);
            string resVal = controller.ExecuteCommand(commandID, args, out bool res);
            if (resVal == ResultMessgeEnum.Success.ToString())
            {
                result = true;
                this.logging.Log(commandID + " " + resVal, ImageService.Logging.Modal.MessageTypeEnum.INFO);
            }
            else
            {
                result = false;
                this.logging.Log(commandID + " " + resVal, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
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
            this.logging.Log("in on command of handler", MessageTypeEnum.INFO);
             //System.Diagnostics.Debugger.Launch();
            if (e.CommandID == (int)CommandEnum.CloseCommand && e.RequestDirPath == null)
            {
                this.dirWatcher.Changed -= this.OnNewFile;
                try
                {
                    this.dirWatcher.EnableRaisingEvents = false;
                    this.dirWatcher.Dispose();
                }
                catch (Exception)
                {
                    this.logging.Log("error while closing the File System Watcher",MessageTypeEnum.INFO );
                }
                //notify every one that signed to the event about the service being closed
               // this.DirectoryClose?.Invoke(this, new DirectoryCloseEventArgs(e.RequestDirPath,null));
                this.logging.Log("file system watch closed", MessageTypeEnum.INFO);
                //closing specific handler
            } else if((e.CommandID == (int)CommandEnum.CloseHandler && e.RequestDirPath.Equals(this.path)))
            {
                this.dirWatcher.Changed -= this.OnNewFile;
                try
                {
                    this.dirWatcher.EnableRaisingEvents = false;
                    this.dirWatcher.Dispose();
                }
                catch (Exception)
                {
                    this.logging.Log("error while closing the File System Watcher", MessageTypeEnum.INFO);
                }
                //notify every one that signed to the event about the service being closed
                //this.DirectoryClose?.Invoke(this, new DirectoryCloseEventArgs(e.RequestDirPath, null));
                this.logging.Log("file system watch closed", MessageTypeEnum.INFO);
            }
            else
            {
                if (e.RequestDirPath == this.path)
                {
                    string res = this.controller.ExecuteCommand(e.CommandID, e.Args, out bool result);
                    if (ResultMessgeEnum.Success.Equals(res))
                    {
                       // this.logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                    }
                    else
                    {
                        this.logging.Log(e.RequestDirPath + res, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
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
            // this.logging.Log("in handler - event on new file reciveced. before" +
            //     " entering the condition" + e.Name, ImageService.Logging.Modal.MessageTypeEnum.INFO);
            //System.Diagnostics.Debugger.Launch();
            //enter here only if the file contains one of the extensions
            if (filters.Contains(strFileExt))
            {
                string[] args = { e.FullPath, e.Name };
                //bool result = true;
                this.controller.ExecuteCommand((int)CommandEnum.NewFileCommand, args, out bool result);
                if (result)
                {
                //    this.logging.Log("in handler - event on new file" +
                    //    " reciveced for" + e.Name + " result of executing command was successful",
                      //  ImageService.Logging.Modal.MessageTypeEnum.INFO);
                }
                else
                {
                    this.logging.Log("in handler - event on new file reciveced for" + e.Name
                        + " executing command failed",
                        ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                }

            }
        }
    }
}
