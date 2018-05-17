using Logs.Controller;
using Logs.Controller.Handlers;
using Infrastructure.Enums;
using Logs.ImageService.Logging;
using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Server
{
    //a class that is responsible of the the handlers
    public class ImageServerManagerOfHandlers : IManagerOfHandlers
    {
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        ILoggingService log;
        IImageController controller;
        /// a list with handlers
        /// </summary>
        private List<IDirectoryHandler> handler;
        public ImageServerManagerOfHandlers(ILoggingService l, IImageController c)
        {
            this.log = l;
            this.controller = c;
            //initialize handlers list
            this.handler = new List<IDirectoryHandler>();
            //create the handler and sign the onClose method to the event
            string folderToListen = ConfigurationManager.AppSettings.Get("Handler");
            string[] folders = folderToListen.Split(';');
            //create handlers for each folder:
            foreach (string folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    this.CreateHandlerForFolder(folder);
                }
            }
        }
        /// <summary>
        /// Creates the handlers for folders defined in the app config.
        /// </summary>
        /// <param name="path">The path.</param>
        private void CreateHandlerForFolder(string path)
        {

            //create the handler and sign the onClose method to the event
            this.handler.Add(new DirectoyHandler(path, this.controller, this.log));
            this.handler.Last().DirectoryClose += this.OnCloseDirectory;
            //register the handler to the event of command recieved 
            this.CommandRecieved += this.handler.Last().OnCommand;
        }
        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            //System.Diagnostics.Debugger.Launch();
            //remove the methods that signed to the events
            foreach (IDirectoryHandler handler in this.handler)
            {
                if (handler.FullPath.Equals(e.DirectoryPath))
                {
                    this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseHandler, null, e.DirectoryPath));
                    this.CommandRecieved -= handler.OnCommand;
                    handler.DirectoryClose -= this.OnCloseDirectory;
                    this.log.Log("removed handler: " + e.DirectoryPath, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                    return;
                }
            }
            this.log.Log("could not find any handlers correspond to path: " + e.DirectoryPath,
                ImageService.Logging.Modal.MessageTypeEnum.INFO);
        }
        //closing of all handlers
        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            //remove the methods that signed to the events
            //System.Diagnostics.Debugger.Launch();
            if (e.CommandID == (int)CommandEnum.CloseCommand)
            {
                this.log.Log("closing all handlers",
                ImageService.Logging.Modal.MessageTypeEnum.INFO);
                foreach (IDirectoryHandler handler in this.handler)
                {
                    this.CommandRecieved -= handler.OnCommand;
                    handler.DirectoryClose -= this.OnCloseDirectory;
                    handler.ExecuteCommand((int)CommandEnum.CloseCommand, null, out bool result);
                    if (result)
                    {
                        this.log.Log("Removed handler successfully, handler: " + handler.FullPath, ImageService.Logging.Modal.MessageTypeEnum.INFO);
                        this.handler.Remove(handler);
                    }
                    else
                    {
                        this.log.Log("couldn't remove handler: " + handler.FullPath, ImageService.Logging.Modal.MessageTypeEnum.FAIL);
                    }
                    
                }
            }
        }
    }
}
