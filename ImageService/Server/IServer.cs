using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    //the server that links between the service (and the log) and the handlers of the directories
    interface IServer
    {
        /// <summary>
        /// Raises when the service is about to close.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DirectoryCloseEventArgs"/> instance containing the event data.</param>
        void OnClose(object sender, DirectoryCloseEventArgs e);

        event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        
    }
}
