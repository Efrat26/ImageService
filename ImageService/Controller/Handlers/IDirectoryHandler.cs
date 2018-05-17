using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Controller.Handlers
{
    /// <summary>
    /// interfacr for object that handles the directories
    /// </summary>
    interface IDirectoryHandler
    {
        // The Event That Notifies that the Directory is being closed
        event EventHandler<DirectoryCloseEventArgs> DirectoryClose;
        /// <summary>
        /// Executes the command specified by command ID.
        /// </summary>
        /// <param name="commandID">The command identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="result">if set to true if the operation was
        /// successful and false otherwise</param>
        /// <returns>a string contains error message (if error occured) or a
        /// success message if it done without errors</returns>
        string ExecuteCommand(int commandID, string[] args, out bool result);

        /// <summary>
        /// Called when there was a command.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="CommandRecievedEventArgs"/> instance containing the event data.</param>
        void OnCommand(object sender, CommandRecievedEventArgs e);
        /// <summary>
        /// Called when there's a new file request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
        void OnNewFile(object sender, FileSystemEventArgs e);
        String FullPath { get; set; }
    }
}
