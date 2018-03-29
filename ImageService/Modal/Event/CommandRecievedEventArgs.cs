using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Modal.Event
{
    /// <summary>
    /// specifies the arguments for a command that was recieved event
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class CommandRecievedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the command ID.
        /// </summary>
        /// <value>
        /// The command ID.
        /// </value>
        public int CommandID { get; set; }
        /// <summary>
        /// Gets or sets the arguments for the command.
        /// </summary>
        /// <value>
        /// The arguments for the command.
        /// </value>
        public string[] Args { get; set; }
        /// <summary>
        /// Gets or sets the request directory.
        /// </summary>
        /// <value>
        /// The request dir path.
        /// </value>
        public string RequestDirPath { get; set; }
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="id">The command identifier.</param>
        /// <param name="args">The arguments for the command.</param>
        /// <param name="path">The path for the requested directory.</param>
        public CommandRecievedEventArgs(int id, string[] args, string path)
        {
            CommandID = id;
            Args = args;
            RequestDirPath = path;
        }
    }

}
