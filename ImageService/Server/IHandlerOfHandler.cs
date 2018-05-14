using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Server
{
    public interface IHandlerOfHandler
    {
        event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        void OnClose(object sender, DirectoryCloseEventArgs e);
    }
}
