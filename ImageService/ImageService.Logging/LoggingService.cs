using ImageService.ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Logging
{
    class LoggingService : ILoggingService
    {
        /// <summary>
        /// Occurs when a message recieved.
        /// </summary>
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        /// <summary>
        /// The text of the message
        /// </summary>
        string msg;
        /// <summary>
        /// type of the message
        /// </summary>
        MessageTypeEnum t;
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of the message.</param>
        public void Log(string message, MessageTypeEnum type)
        {
            MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(message, t));
        }
    }
}
