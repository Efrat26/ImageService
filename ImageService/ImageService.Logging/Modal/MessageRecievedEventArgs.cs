using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Logging.Modal
{
    /// <summary>
    /// the args for the message recieved function
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class MessageRecievedEventArgs : EventArgs
    {
        /// <summary>
        /// Status property - gets or sets the status.
        /// </summary>
        /// <value>
        /// The status of the message
        /// </value>
        public MessageTypeEnum Status { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="msg">The message text.</param>
        /// <param name="t">The type/status of the message.</param>
        public MessageRecievedEventArgs(string msg, MessageTypeEnum t)
        {
            Message = msg;
            Status = t;
        }


    }
}
