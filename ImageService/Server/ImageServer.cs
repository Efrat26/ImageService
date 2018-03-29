using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.ImageService.Infrastructure.Enums;
using ImageService.ImageService.Logging;
using ImageService.ImageService.Logging.Modal;
using ImageService.Modal;
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
    class ImageServer : IServer
    {
        #region Members
        private IImageController m_controller;
        private ImageService.Logging.ILoggingService m_logging;
        private List<IDirectoryHandler> m_handler;
        #endregion
        #region Properties
        // The event that notifies about a new Command being recieved
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          
        #endregion
        public ImageServer(ILoggingService l)
        {
            //thr logger of the service
            this.m_logging = l;
            //create image model
            this.m_controller = new ImageController(new ImageServiceModal());
            //initialize handlers list
            this.m_handler = new List<IDirectoryHandler>();
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
            this.m_logging.Log("Hello frm server", ImageService.Logging.Modal.MessageTypeEnum.INFO);
        }
        public void OnClose(object sender, DirectoryCloseEventArgs e)
        {
            this.m_logging.Log("in on close of server", MessageTypeEnum.INFO);
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs((int)CommandEnum.CloseCommand,null,null));
            this.m_logging.Log(e.Message, MessageTypeEnum.INFO);
            //remove the methods that signed to the events
            this.CommandRecieved -= ((IDirectoryHandler)sender).OnCommand;
            ((IDirectoryHandler)sender).DirectoryClose -= this.OnClose;
        }
        public void CreateHandlerForFolder(string path)
        {
           
            //create the handler and sign the onClose method to the event
            this.m_handler.Add( new DirectoyHandler(path, this.m_controller, this.m_logging));
            this.m_handler.Last().DirectoryClose += this.OnClose;
            //register the handler to the event of command recieved 
            this.CommandRecieved += this.m_handler.Last().OnCommand;
        }
    }
}
