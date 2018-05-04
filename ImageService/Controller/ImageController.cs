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
        /// <summary>
        /// The Modal Object that does the action 
        /// </summary>
        private IImageServiceModal modal;
        /// <summary>
        /// a commands dictionary
        /// </summary>
        private Dictionary<int, ICommand> commands;
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageController"/> class.
        /// </summary>
        /// <param name="modal">The modal.</param>
        public ImageController(IImageServiceModal m)
        {
            // Storing the Modal Of The System
            modal = m;
            //creates the directory with the commands
            commands = new Dictionary<int, ICommand>()
            {
                // For Now will contain NEW_FILE_COMMAND
                {(int)CommandEnum.NewFileCommand, new NewFileCommand(this.modal)},
                {(int)CommandEnum.NewFileCommand, new GetConfigCommand(this.modal)},
                {(int)CommandEnum.NewFileCommand, new LogCommand(this.modal)},
                { (int)CommandEnum.CloseCommand, new CloseCommand(this.modal)}
            };
        }
        /// <summary>
        /// Executes the command in a task.
        /// </summary>
        /// <param name="commandID">The command identifier.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="resultSuccesful">if set to <c>true</c> [result succesful].</param>
        /// <returns></returns>
        public string ExecuteCommand(int commandID, string[] args, out bool resultSuccesful)
        {
            bool result;
            //this.commands[commandID].Execute(args, out result);
            Task<bool> t = new Task<bool>(() =>
            {Task.Delay(1000); this.commands[commandID].Execute(args, out result);return result; });
            t.Start();
            //t.Wait();
            //wait for the result from the task and return a boolean accordingly
            resultSuccesful = t.Result;
            if (resultSuccesful)
            {
                return ResultMessgeEnum.Success.ToString();
            }
            else
            {
                return ResultMessgeEnum.Fail.ToString();
            }
        }
        //currently doing nothing
        public void OnCloseOfService(object sender, EventArgs e)
        {
            
        }
    }
}
