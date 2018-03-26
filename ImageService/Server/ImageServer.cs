using ImageService.Controller;
using ImageService.Controller.Handlers;
using ImageService.ImageService.Logging;
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
        public ImageServer()
        {

        }
        public ImageServer(string PathToFolderToListen)
        {

        }
    }
}
