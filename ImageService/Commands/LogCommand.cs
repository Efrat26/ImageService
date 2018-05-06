using ImageService.Modal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Commands
{
    public class LogCommand : ICommand
    {
        /// <summary>
        /// The modal that does the operation
        /// </summary>
        private IImageServiceModal modal;

        /// <param name="m">The modal.</param>
        public LogCommand(IImageServiceModal m)
        {
            modal = m;
        }
        /// <summary>
        /// Executes the specified command (by command ID).
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <param name="result">if set to true if the execution was successful
        /// and false otherwise.</param>
        /// <returns>
        /// string that contains the error message or a success message
        /// </returns>
        public string Execute(string[] args, out bool result)
        {
            // The String Will Return the New Path if result = true, and will return the error message
            string path = this.modal.AddFile(args[0], out bool res);
            if (res == true)
            {
                result = true;
                return path;
            }
            else
            {
                result = false;
                return path;
            }
        }
    }
}
