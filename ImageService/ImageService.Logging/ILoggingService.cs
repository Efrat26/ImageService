﻿using ImageService.ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Logging
{
    /// <summary>
    /// interface for the logging service
    /// </summary>
    interface ILoggingService
    {       
        /// <summary>
        /// Occurs when a message recieved.
        /// </summary>
        event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of the message.</param>
        void Log(string message, MessageTypeEnum type);
    }
}
