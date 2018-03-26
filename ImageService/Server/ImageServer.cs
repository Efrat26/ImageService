﻿using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.ImageService.Logging;
using ImageService.ImageService.Logging.Modal;
using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    class ImageServer
    {
        #region Members
        private IImageController m_controller;
        private ImageService.Logging.ILoggingService m_logging;
        private IDirectoryHandler m_handler;
        #endregion
        #region Properties
        // The event that notifies about a new Command being recieved
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          
        #endregion
        public ImageServer(string PathToFolderToListen, ILoggingService l)
        {
            //thr logger of the service
            this.m_logging = l;
            //create image model
            this.m_controller = new ImageController();
            //create the handler and sign the onClose method to the event
            this.m_handler = new DirectoyHandler(PathToFolderToListen, this.m_controller);
            this.m_handler.DirectoryClose += this.OnClose;
            //register the handler to the event of command recieved 
            this.CommandRecieved += this.m_handler.OnCommand;
        
        }
        public void OnClose(object sender, DirectoryCloseEventArgs e)
        {
            this.CommandRecieved?.Invoke(this, new CommandRecievedEventArgs(0, null, e.DirectoryPath));
            this.m_logging.Log(e.Message, MessageTypeEnum.INFO);
            //remove the methods that signed to the events
            this.CommandRecieved -= ((IDirectoryHandler)sender).OnCommand;
            ((IDirectoryHandler)sender).DirectoryClose -= this.OnClose;
        }
    }
}
