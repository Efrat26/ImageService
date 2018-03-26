using ImageService.Commands;
using ImageService.ImageService.Infrastructure.Enums;
using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Controller
{
    class ImageController : IImageController
    {
        // The Modal Object that does the action
        private IImageServiceModal m_modal;                     
        private Dictionary<int, ICommand> commands;

        public ImageController(IImageServiceModal modal)
        {
            // Storing the Modal Of The System
            m_modal = modal;
            commands = new Dictionary<int, ICommand>()
            {
                // For Now will contain NEW_FILE_COMMAND
                {(int)CommandEnum.NewFileCommand, new NewFileCommand(this.m_modal)},
                { (int)CommandEnum.CloseCommand, new CloseCommand(this.m_modal)}
            };
        }
        public string ExecuteCommand(int commandID, string[] args, out bool resultSuccesful)
        {
            bool result;
            //this.commands[commandID].Execute(args, out result);
            Task<bool> t = new Task<bool>(() =>
            { this.commands[commandID].Execute(args, out result);return result; });
            t.Start();
            resultSuccesful = t.Result;
            if (resultSuccesful)
            {
                return "success";
            }
            else
            {
                return "error";
            }
        }
    }
}
