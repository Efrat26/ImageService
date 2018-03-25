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
        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;
        public void Log(string message, MessageTypeEnum type)
        {

        }
    }
}
