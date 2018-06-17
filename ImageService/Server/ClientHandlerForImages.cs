using Logs.Controller;
using Logs.ImageService.Logging.Modal;
using Logs.Modal.Event;
using Logs.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    class ClientHandlerForImages : IClientHandler
    {
        public event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        public event EventHandler<DirectoryCloseEventArgs> CloseCommand;
        
        public void HandleClient(TcpClient c)
        {
          
        }

        public void OnCloseDirectory(object sender, DirectoryCloseEventArgs e)
        {
            
        }

        public void OnCommandRecieved(object sender, CommandRecievedEventArgs e)
        {
            
        }

        public void OnLogMessageRecieved(object sender, MessageRecievedEventArgs e)
        {
            
        }

        public void SetController(IImageController c)
        {
           
        }
    }
}
