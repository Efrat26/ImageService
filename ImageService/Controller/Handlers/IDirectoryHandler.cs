using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller.Handlers
{
    interface IDirectoryHandler
    {
        // Executing the Command Requet
        string ExecuteCommand(int commandID, string[] args, out bool result);         

    }
}
