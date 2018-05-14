using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
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
            //remove the methods that signed to the events
            foreach (IDirectoryHandler handler in this.handler)
            {
                if (handler.Path.Equals(e.DirectoryPath))
                {
                    this.CommandRecieved -= handler.OnCommand;
                    handler.DirectoryClose -= this.OnCloseDirectory;
                    break;
                }
            }
        }
        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            //remove the methods that signed to the events
            if (e.CommandID == (int)CommandEnum.CloseCommand)
            {
                foreach (IDirectoryHandler handler in this.handler)
                {
                    this.CommandRecieved -= handler.OnCommand;
                    handler.DirectoryClose -= this.OnCloseDirectory;
                }
            }
        }
    }
}
