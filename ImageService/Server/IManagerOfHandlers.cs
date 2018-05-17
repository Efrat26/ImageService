using Logs.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logs.Server
{
    public interface IManagerOfHandlers
    {
        event EventHandler<CommandRecievedEventArgs> CommandRecieved;
        void OnCloseDirectory(object sender, DirectoryCloseEventArgs e);
        void OnCommandRecieved(object sender, CommandRecievedEventArgs e);
    }
}
