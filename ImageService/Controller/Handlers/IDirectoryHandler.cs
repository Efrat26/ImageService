using ImageService.Modal.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller.Handlers
{
    interface IDirectoryHandler
    {
        // The Event That Notifies that the Directory is being closed
        event EventHandler<DirectoryCloseEventArgs> DirectoryClose;
        // Executing the Command Requet
        string ExecuteCommand(int commandID, string[] args, out bool result);

        void OnCommand(object sender, CommandRecievedEventArgs e);
        //on creation of file method
        void OnNewFile(object sender, EventArgs e);
    }
}
