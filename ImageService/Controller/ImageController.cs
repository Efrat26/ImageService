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
                {(int)CommandEnum.GetConfigCommand, new GetConfigCommand(this.modal)},
                {(int)CommandEnum.LogCommand, new LogCommand(this.modal)},
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
            bool res;
            String operationResult, result;
            //this.commands[commandID].Execute(args, out result);
            Task<String> t = new Task<String>(() =>
            {Task.Delay(1000); operationResult = this.commands[commandID].Execute(args, out res);return operationResult; });
            t.Start();
            //t.Wait();
            //wait for the result from the task and return a boolean accordingly
            result = t.Result;
            if (!result.Equals(ResultMessgeEnum.Fail.ToString()))
            {
                resultSuccesful = true;
                return result;
            }
            else
            {
                resultSuccesful = false;
                return ResultMessgeEnum.Fail.ToString();
            }
        }
        //currently doing nothing
        public void OnCloseOfService(object sender, EventArgs e)
        {
           
        }
    }
}
