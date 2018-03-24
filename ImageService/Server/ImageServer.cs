using ImageService.Controller;
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
        private ImageService.ImageService.Logging.ILoggingService m_logging;
        #endregion

        #region Properties
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;          // The event that notifies about a new Command being recieved
        #endregion
    }
}
