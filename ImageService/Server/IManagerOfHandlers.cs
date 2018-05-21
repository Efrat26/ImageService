using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Server
{
    /// <summary>
    /// interface for a handler manger object
    /// </summary>
    public interface IManagerOfHandlers
    {
        /// <summary>
        /// Occurs when a command recieved.
        /// </summary>
        event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        /// <summary>
        /// Called when directory closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DirectoryCloseEventArgs"/> instance containing the event data.</param>
        void OnCloseDirectory(object sender, DirectoryCloseEventArgs e);
        /// <summary>
        /// Called when a command recieved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CommandRecievedEventArgs"/> instance containing the event data.</param>
        void OnCommandRecieved(object sender, CommandRecievedEventArgs e);
    }
}
