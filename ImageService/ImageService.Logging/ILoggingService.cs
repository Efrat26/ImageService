using ImageService.ImageService.Logging.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.ImageService.Logging
{
    interface ILoggingService
    {
        event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        void Log(string message, MessageTypeEnum type);           // Logging the Message
    }
}
